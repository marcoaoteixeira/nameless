using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;
using Nameless.Environment;

namespace Nameless.FileStorage.FileSystem {

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

		#region Public Properties

		public string AppDataFolderName { get; set; } = "App_Data";

		#endregion

		#region Protected Override Methods

		/// <inheritdoc/>
		protected override void Load(ContainerBuilder builder) {
			builder
				.RegisterType<FileSystemStorage>()
				.As<IFileStorage>()
				.OnPreparing(OnPreparingFileStorage)
				.SetLifetimeScope(LifetimeScopeType.PerScope);

			base.Load(builder);
		}

		#endregion

		#region Private Static Methods

		private void OnPreparingFileStorage(PreparingEventArgs args) {
			var hostEnvironment = args.Context.ResolveOptional<IHostEnvironment>();
			var options = args.Context.ResolveOptional<FileSystemStorageOptions>() ?? new();

			string path;
			if (string.IsNullOrWhiteSpace(options.Root)) {
				path = hostEnvironment != null
					? Path.Combine(hostEnvironment.ApplicationBasePath, AppDataFolderName.OnBlank("App_Data"))
					: Path.Combine(typeof(FileStorageModule).Assembly.GetDirectoryPath(), AppDataFolderName.OnBlank("App_Data"));
			} else { path = options.Root; }

			options.Root = PathHelper.Normalize(path);

			args.Parameters = args.Parameters.Union(new Parameter[] {
				TypedParameter.From(options)
			});
		}

		#endregion
	}
}
