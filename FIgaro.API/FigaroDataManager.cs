using System;
using System.Diagnostics;
using Figaro;
using Figaro.Configuration.Factory;

namespace Figaro.API
{
    /// <summary>
    /// Sample data management object for using the Figaro database library.
    /// </summary>
    public class FigaroDataManager : IDisposable
    {
        /// <summary>
        /// Gets or sets the database environment.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/api/Figaro.FigaroEnv.html"/>
        public FigaroEnv Environment { get; }

        /// <summary>
        /// Gets or sets the database manager.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/api/Figaro.XmlManager.html"/>
        public XmlManager Manager { get; }

        /// <summary>
        /// Gets the Figaro database.
        /// </summary>
        /// <seealso cref="http://help.bdbxml.net/api/Figaro.Container.html"/>
        public Container Database { get; }
        /// <summary>
        /// Initialize the Figaro data objects via Figaro.Configuration 
        /// </summary>
        public FigaroDataManager()
        {
            //The Figaro.Configuration will create the FigaroEnv object for the XmlManager it is 
            // assigned to, so we can simply retrieve the reference to it from the manager and 
            // avoid creating multiple instances and adding additional, unnecessary reference 
            // instances. Otherwise, we'd simply create it first and assign to the manager.
            Manager = ManagerFactory.Create("demoMgr");
            Environment = Manager.Environment;

            //configure logging, progress and panic events
            Environment.OnErr += Environment_OnErr;
            Environment.OnMessage += Environment_OnMessage;
            Environment.OnProcess += Environment_OnProcess;
            Environment.OnProgress += Environment_OnProgress;
            Environment.ErrEventEnabled = true;
            Environment.MessageEventEnabled = true;
            Environment.ProcessEventEnabled = true;
            Environment.ProgressEventEnabled = true;

            //http://help.bdbxml.net/api/Figaro.XmlManager.html#Figaro_XmlManager_CreateTransaction
            Database = Manager.OpenContainer(ContainerConfigFactory.Create("demoMgr", "demo"));

        }
        public ulong GetRecordCount()
        {
            return Database.GetNumDocuments();
        }

        /// <summary>
        /// Inserts an XML record pulled from a URL.
        /// </summary>
        /// <param name="url">The URL to extract XML from.</param>
        /// <returns>The name of the record.</returns>
        public string InsertRecordFromUrl(string url)
        {
            var name = "record" + DateTime.Now.ToFileTimeUtc();
            var stm = Manager.CreateUrlInputStream(string.Empty, url);
            var doc = Manager.CreateDocument();
            doc.SetContentAsInputStream(stm);
            doc.Name = name;

            //add metadata to the record.
            doc.SetMetadata("http://schemas.bdbxml.net/metadata", "CreatedDate",
                new XmlValue(DateTime.Now.ToString()));
                Database.PutDocument(doc, Manager.CreateUpdateContext());
            return name;

        }

        /// <summary>
        /// Insert a System.Xml.XmlDocument into the database.
        /// </summary>
        /// <param name="doc">The document to insert.</param>
        /// <returns>The document name, for lookup purposes.</returns>
        public string InsertRecord(System.Xml.XmlDocument doc)
        {
            var name = "record" + DateTime.Now.ToFileTimeUtc();
            var xml = Manager.CreateDocument(doc);
            xml.Name = name;
            //add metadata to the record.
            xml.SetMetadata("http://schemas.bdbxml.net/metadata","CreatedDate",
            new XmlValue(DateTime.Now.ToString()));
            Database.PutDocument(xml, Manager.CreateUpdateContext());                                
            return name;
        }

        /// <summary>
        /// Environment progress event handler. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnProgress(object sender, ProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case FeedbackEvent.Other:
                    Trace.WriteLine($"feedback event {e.PercentComplete}%");
                    break;
                case FeedbackEvent.Recover:
                    Trace.WriteLine($"feedback event {e.PercentComplete}%");
                    break;
            }
        }

        /// <summary>
        /// Handle panic and write failure events here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnProcess(object sender, ProcessEventArgs e)
        {
            if (e.EventType == EnvironmentEvent.Panic)
                Trace.WriteLine("A panic event occurred!", "Panic");
            if (e.EventType == EnvironmentEvent.WriteEventFailure)
                Trace.WriteLine("A write event failed to occur!", "Fatal");
        }

        /// <summary>
        /// For diagnostic message events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnMessage(object sender, MsgEventArgs e)
        {
            Trace.WriteLine($"{e.Message}", "Message");
        }

        /// <summary>
        /// For writing error related events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Environment_OnErr(object sender, ErrEventArgs e)
        {
            Trace.WriteLine($"{e.Prefix}: {e.Message}", "Error");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // gracefully exit from our resources. Note that these need to be closed in 
            // the sequence shown.
            Database.Dispose();
            Manager?.Dispose();
            Environment?.Dispose();
        }
    }
}
