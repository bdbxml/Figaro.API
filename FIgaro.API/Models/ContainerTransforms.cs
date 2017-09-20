using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Figaro.API.Models
{
    public class ContainerTransforms
    {
        public static ContainerConfig CreateConfig(ContainerElementType cnt)
        {
            // if the alias isn't specified use the name and remove ".dbxml"
            return new ContainerConfig
            {
                Path = cnt.name,
                AllowCreate = cnt.allowCreate,
                AllowValidation = cnt.allowValidation,
                Checksum = false,
                CompressionEnabled = cnt.compression,
                ConfigurationName = cnt.name,
                Encrypted = cnt.encrypted,
                ContainerType = (Figaro.XmlContainerType)cnt.containerType,
                ExclusiveCreate = cnt.exclusiveCreate,
                IndexNodes = (Figaro.ConfigurationState)cnt.indexNodes,
                MultiVersion = cnt.multiVersion,
                NoMMap = cnt.noMMap,
                ReadUncommitted = cnt.readUncommitted,
                SequenceIncrement = cnt.sequenceIncrement,
                Statistics = Figaro.ConfigurationState.On,
                Threaded = cnt.threaded,
                TransactionNotDurable = cnt.transactionNotDurable,
                Transactional = cnt.transactional
            };
        }
        public static ConcurrentBag<ContainerConfig> CreateConfig(ContainerElementCollectionType containers)
        {
            var cfgs = new List<ContainerConfig>();
            foreach (var cnt in containers.Container)
            {
                // if the alias isn't specified use the name and remove ".dbxml"
                var cfg = new ContainerConfig
                {
                    Path = cnt.name,
                    AllowCreate = cnt.allowCreate,
                    AllowValidation = cnt.allowValidation,
                    Checksum = false,
                    CompressionEnabled = cnt.compression,
                    Encrypted = cnt.encrypted,
                    ConfigurationName = cnt.name,
                    ContainerType = (Figaro.XmlContainerType)cnt.containerType,
                    ExclusiveCreate = cnt.exclusiveCreate,
                    IndexNodes = (Figaro.ConfigurationState)cnt.indexNodes,
                    MultiVersion = cnt.multiVersion,
                    NoMMap = cnt.noMMap,
                    ReadUncommitted = cnt.readUncommitted,
                    SequenceIncrement = cnt.sequenceIncrement,
                    Statistics = Figaro.ConfigurationState.On,
                    Threaded = cnt.threaded,
                    TransactionNotDurable = cnt.transactionNotDurable,
                    Transactional = cnt.transactional
                };
                cfgs.Add(cfg);
            }
            return new ConcurrentBag<ContainerConfig>(cfgs);

        }
    }
}