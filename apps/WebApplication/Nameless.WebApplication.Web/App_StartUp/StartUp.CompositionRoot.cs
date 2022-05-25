using Autofac;
using Nameless.Caching.InMemory;
using Nameless.Environment.System;
using Nameless.FileStorage.System;
using Nameless.Logging.Log4net;
using Nameless.NHibernate;
using Nameless.Serialization.Json;
using Nameless.WebApplication.Web.Infrastructure;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder) {
            builder
                .RegisterModule(new CachingModule())
                .RegisterModule(new EnvironmentModule())
                .RegisterModule(new FileStorageModule())
                .RegisterModule(new LoggingModule())
                .RegisterModule(new NHibernateModule())
                .RegisterModule(new SerializationModule())
                .RegisterModule(new AppModule());
        }

        #endregion
    }
}
