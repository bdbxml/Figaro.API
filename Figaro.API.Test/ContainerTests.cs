using System;
using System.Collections.Concurrent;
using System.IO;
using Xunit;
using Figaro.API.Models;
using Nancy.Testing;
using Xunit.Abstractions;

namespace Figaro.API.Test
{
    public class ContainerTests
    {
        private readonly ITestOutputHelper _output;

        public ContainerTests(ITestOutputHelper helper)
        {
            _output = helper;
        }

        [Fact]
        public void container_config_conversion_test()
        {            
            var bag = get_the_bag(get_multiple_container_collection());
            Assert.True(bag.Count == 3 );

            bag = get_the_bag(get_single_container_collection());
            
            Assert.True(bag.Count == 1);
        }

        [Fact]
        public void container_create_test()
        {
            var bag = get_the_bag(get_single_container_collection());
            
            var bs = new FigaroApiBootstrapper(Globals.RootPath);
            var bw = new Browser(bs);

            var result = bw.Put("/containers", with =>
            {
                with.XMLBody(bag);
            });

            Assert.True(result.IsCompleted);
            Assert.False(result.IsFaulted);
        }

        [Fact]
        public void root_path_test()
        {
            var bs = new FigaroApiBootstrapper(Globals.RootPath);
            var dir = new DirectoryInfo(bs.RootPath);
            var basePath = dir.Parent.Parent.Parent.FullName;
            _output.WriteLine("root path: " + basePath);

            _output.WriteLine("appDomain base directory: " + AppDomain.CurrentDomain.BaseDirectory);

        }

        private ConcurrentBag<ContainerConfig> get_the_bag(ContainerElementCollectionType collection)
        {
            return ContainerTransforms.CreateConfig(collection);
        }

        private ContainerElementCollectionType get_single_container_collection()
        {
            return new ContainerElementCollectionType
            {
                Container = new[]
                {
                    new ContainerElementType()
                    {
                        alias = "con1",
                        allowCreate = true,
                        allowValidation = false,
                        compression = false,
                        containerType = Models.XmlContainerType.NodeContainer,
                        encrypted = false,
                        exclusiveCreate = false,
                        indexNodes = Models.ConfigurationState.Off,
                        name = "con1"
                    }
                }
            };
        }
        private ContainerElementCollectionType get_multiple_container_collection()
        {
            return new ContainerElementCollectionType
            {
                Container = new[]
                {
                    new ContainerElementType()
                    {
                        alias = "con1",
                        allowCreate = true,
                        allowValidation = false,
                        compression = false,
                        containerType = Models.XmlContainerType.NodeContainer,
                        encrypted = false,
                        exclusiveCreate = false,
                        indexNodes = Models.ConfigurationState.Off,
                        name = "con1"
                    },
                    new ContainerElementType() {alias = "con2", name = "con2"}, // just use defaults for everything else
                    new ContainerElementType() {alias = "con3", name = "con3"}
                }
            };

        }
    }
}