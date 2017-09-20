using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using Figaro;
using Figaro.Configuration.Factory;
using Figaro.API.Exceptions;
using Figaro.API.Models;
using Nancy;

namespace Figaro.API
{
    /// <summary>
    /// Sample data management object for using the Figaro database library.
    /// </summary>
    /// <remarks>Renamed from FigaroDataManager so that it wouldn't get overwritten by Figaro package updates.</remarks>
    public class FigaroDataContext : IDisposable
    {
        /// <summary>
        /// Gets or sets the database environment.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/html/5f9eb5f1-764f-4a58-af59-fed2c87ad6bc.htm"/>
        public FigaroEnv Environment { get; }

        /// <summary>
        /// Gets or sets the database manager.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/html/514038c7-547b-476e-8bda-69428f315172.htm"/>
        public XmlManager Manager { get; }

        /// <summary>
        /// Gets a <see cref="ConcurrentDictionary{TKey,TValue}"/> of <see cref="Container"/> objects currently open in the API.
        /// </summary>
        public ConcurrentDictionary<string, Container> Containers { get; }

        protected string BasePath;

        /// <summary>
        /// Initialize the Figaro data objects via Figaro.Configuration 
        /// </summary>
        public FigaroDataContext(string rootPath)
        {
            BasePath = rootPath;
            //The Figaro.Configuration will create the FigaroEnv object for the XmlManager it is 
            // assigned to, so we can simply retrieve the reference to it from the manager and 
            // avoid creating multiple instances and adding additional, unnecessary reference 
            // instances. Otherwise, we'd simply create it first and assign to the manager.

            Containers = new ConcurrentDictionary<string, Container>();

            Environment = new FigaroEnv();

            Environment.SetCreateDirectory(Path.Combine(rootPath,"App_Data"));
            Environment.AddDataDirectory(Path.Combine(rootPath,"App_Data","Data"));
            Environment.SetLogDirectory(Path.Combine(rootPath,"App_Data","Log"));
            Environment.SetTempDirectory(Path.Combine(rootPath,"App_Data","Temp"));
            Environment.SetMessageFile(Path.Combine(rootPath,"App_Data","msg.log"));
            Environment.SetErrorFile(Path.Combine(rootPath,"App_Data","err.log"));

            Environment.SetCacheMax(new EnvCacheSize(2,0));
            Environment.SetCacheSize(new EnvCacheSize(1,0), 2);
         
            Environment.SetVerbose(VerboseOption.AllFileOps, true);

            // Configuring the Locking Subsystem: http://help.bdbxml.net/articles/configuring-the-locking-subsystem.html
            //http://http://help.bdbxml.net/api/Figaro.FigaroEnv.html#Figaro_FigaroEnv_SetLockPartitions_System_UInt32_
            Environment.SetLockPartitions(20);
            // Configuring Deadlock Detection: http://help.bdbxml.net/articles/configuring-deadlock-detection.html
            // DeadlockDetectType Enumeration: http://help.bdbxml.net/api/Figaro.DeadlockDetectType.html
            Environment.DeadlockDetectPolicy = DeadlockDetectType.Oldest;
            Environment.SetMaxLockers(5000);
            Environment.SetMaxLocks(50000);
            Environment.SetMaxLockedObjects(50000);

            Environment.SetMaxFileDescriptors(100);
            Environment.SetLogBufferSize(1024 * 1024 * 750);
            Environment.MaxLogSize = 1024 * 1024 * 100;

            Environment.SetMaxTransactions(500);
            Environment.SetLogOptions(EnvLogOptions.Direct, true);
            Environment.SetEnvironmentOption(EnvConfig.DirectDB, true);

            Environment.SetTimeout(10000, EnvironmentTimeoutType.Transaction);
            Environment.SetTimeout(1000, EnvironmentTimeoutType.Lock);


            Environment.Open(Path.Combine(rootPath,"App_Data"), EnvOpenOptions.SystemSharedMem | EnvOpenOptions.Recover | EnvOpenOptions.TransactionDefaults | EnvOpenOptions.Create | EnvOpenOptions.Thread);



            Manager = new XmlManager(Environment,ManagerInitOptions.AllOptions);



            //configure logging, progress and panic events
            //Environment.OnErr += Environment_OnErr;
            //Environment.OnMessage += Environment_OnMessage;
            //Environment.OnProcess += Environment_OnProcess;
            //Environment.OnProgress += Environment_OnProgress;
            //Environment.ErrEventEnabled = true;
            //Environment.MessageEventEnabled = true;
            //Environment.ProcessEventEnabled = true;
            //Environment.ProgressEventEnabled = true;
        }

        /// <summary>
        /// Handle the POST operation of a container configuration to an account by creating the containers requested.
        /// </summary>
        /// <param name="config"></param>
        /// <returns>True if the operation was handled successfully; otherwise, false.</returns>
        /// <remarks>
        /// <para>
        /// If the container exists in the collection, then it is already created an a 
        /// </para>
        /// </remarks>
        /// <exception cref="ContainerExistsException">If the container already exists.</exception>
        public HttpStatusCode PostContainersToAccount(ContainerElementCollectionType collection, XmlTransaction tx)
        {
            HttpStatusCode ret = HttpStatusCode.OK;
            foreach (var config in collection.Container)
            {
                var cfg = ContainerTransforms.CreateConfig(config);
                if (Containers.ContainsKey(config.name))
                {
                    //TODO: Log and/or throw? take parameter to determine behavior?
                    ret = HttpStatusCode.Found;
                    continue;
                }

                var c = Manager.CreateContainer(tx, cfg);
                c.AddAlias(config.alias);
                
                //TODO: add indexing??

            }

            return ret;

        }

        public HttpStatusCode DeleteContainersFromAccount(XmlTransaction tx)
        {
            var ret = HttpStatusCode.OK;
            try
            {
                foreach (var container in Containers)
                {
                    Manager.RemoveContainer(tx, container.Value.Name);
                    Container c;
                    Containers.TryRemove(container.Key, out c);
                    c.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return ret;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // gracefully exit from our resources. Note that these need to be closed in 
            // the sequence shown.
            foreach (var container in Containers)
            {
                Container c;
                Containers.TryRemove(container.Key, out c);
                c?.Dispose();
            }
            Manager?.Dispose();
            Environment?.Dispose();
        }
    }
}
