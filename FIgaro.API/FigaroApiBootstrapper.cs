using Nancy;
using Nancy.TinyIoc;

namespace Figaro.API
{
    public sealed class FigaroApiBootstrapper: DefaultNancyBootstrapper
    {
        private readonly string _rootPath;

        /// <summary>
        /// Initialize the bootstrapper with the base directory for the web application.
        /// </summary>
        /// <param name="rootPath">The base directory of the web application.</param>
        public FigaroApiBootstrapper(string rootPath)
        {
            _rootPath = rootPath;
        }

        public FigaroApiBootstrapper()
        {
            _rootPath = RootPathProvider.GetRootPath();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(new FigaroDataContext(_rootPath));
        }

        public string RootPath => RootPathProvider.GetRootPath();
    }
}
