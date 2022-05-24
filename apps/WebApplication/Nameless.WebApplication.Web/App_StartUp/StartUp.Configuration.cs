using Nameless.AspNetCore.Extensions;
using Nameless.FileStorage.FileSystem;
using Nameless.Logging.Log4net;
using Nameless.NHibernate;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void ConfigureConfiguration(IServiceCollection services) {
            var fileSystemStorageOptions = Configuration
                .GetSection(nameof(FileSystemStorageOptions).Replace("Options", string.Empty))
                .Get<FileSystemStorageOptions>() ?? new FileSystemStorageOptions {
                    Root = typeof(StartUp).Assembly.GetDirectoryPath()!
                };
            services.ConfigureOptions(Configuration, () => fileSystemStorageOptions);

            var loggingOptions = Configuration
                .GetSection(nameof(LoggingOptions).Replace("Options", string.Empty))
                .Get<LoggingOptions>() ?? new();
            services.ConfigureOptions(Configuration, () => loggingOptions);

            var nhibernateOptions = Configuration
                .GetSection(nameof(NHibernateOptions).Replace("Options", string.Empty))
                .Get<NHibernateOptions>() ?? new();
            services.ConfigureOptions(Configuration, () => nhibernateOptions);
        }

        #endregion
    }
}
