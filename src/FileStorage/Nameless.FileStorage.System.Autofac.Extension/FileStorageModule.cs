using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;
using Nameless.Environment;

namespace Nameless.FileStorage.System {

    /// <summary>
    /// The FileStorage service registration.
    /// </summary>
    /// <remarks>
    /// It will use the <see cref="IHostEnvironment.ApplicationBasePath" />,
    /// combined with "App_Data", as the root path for the
    /// <see cref="IFileStorage" /> implementation.
    /// If <see cref="IHostEnvironment.ApplicationBasePath" /> is not
    /// present, it will use the <see cref="FileStorageSettings.Root" />
    /// property instead.
    /// </remarks>
    public sealed class FileStorageModule : ModuleBase {

        #region Private Constants

        private const string APP_DATA_FOLDER_NAME = "App_Data";

        #endregion

        #region Public Properties

        public string AppDataFolderName { get; set; } = APP_DATA_FOLDER_NAME;

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder
                .RegisterType<SystemFileStorage>()
                .As<IFileStorage>()
                .OnPreparing(OnPreparingFileStorage)
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion

        #region Private Static Methods

        private void OnPreparingFileStorage(PreparingEventArgs args) {
            var hostEnvironment = args.Context.ResolveOptional<IHostEnvironment>();
            var options = args.Context.ResolveOptional<FileStorageOptions>() ?? new();

            string path;
            if (string.IsNullOrWhiteSpace(options.Root)) {
                path = hostEnvironment != null
                    ? Path.Combine(hostEnvironment.ApplicationBasePath, AppDataFolderName.OnBlank(APP_DATA_FOLDER_NAME))
                    : Path.Combine(typeof(FileStorageModule).Assembly.GetDirectoryPath(), AppDataFolderName.OnBlank(APP_DATA_FOLDER_NAME));
            } else { path = options.Root; }

            options.Root = PathHelper.Normalize(path);

            args.Parameters = args.Parameters.Union(new Parameter[] {
                TypedParameter.From(options)
            });
        }

        #endregion
    }
}
