namespace Nameless.Logging.NLog {

    public sealed class LoggingOptions {

		#region Public Constants

		public const string DEFAULT_CONFIGURATION_FILE_NAME = "log4net.config";

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the configuration file name.
		/// </summary>
		public string ConfigurationFileName { get; set; } = DEFAULT_CONFIGURATION_FILE_NAME;
		/// <summary>
		/// Gets or sets the repository name.
		/// </summary>
		public string? RepositoryName { get; set; }
		/// <summary>
		/// Gets or sets a value indicating whether the configuration file is watched.
		/// </summary>
		public bool ReloadOnChange { get; set; } = true;

		#endregion
	}
}
