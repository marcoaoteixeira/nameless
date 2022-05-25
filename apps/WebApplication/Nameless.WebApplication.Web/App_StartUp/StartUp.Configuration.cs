using Nameless.AspNetCore.Extensions;
using Nameless.FileStorage.System;
using Nameless.Logging.Log4net;
using Nameless.NHibernate;
using Nameless.WebApplication.Web.Security;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void ConfigureConfiguration(IServiceCollection services) {
            services.ConfigureOptions(Configuration, () => GetConfigurationFor<FileStorageOptions>(Configuration));
            services.ConfigureOptions(Configuration, () => GetConfigurationFor<LoggingOptions>(Configuration));
            services.ConfigureOptions(Configuration, () => GetConfigurationFor<NHibernateOptions>(Configuration));
            services.ConfigureOptions(Configuration, () => GetConfigurationFor<JwtOptions>(Configuration));
        }

        #endregion

        #region Private Methods

        private static TConfiguration GetConfigurationFor<TConfiguration>(IConfiguration configuration) where TConfiguration : class, new() {
            var sectionName = typeof(TConfiguration).Name.Replace("Options", string.Empty);

            return configuration
                .GetSection(sectionName)
                .Get<TConfiguration>() ?? new();
        }

        #endregion
    }
}
