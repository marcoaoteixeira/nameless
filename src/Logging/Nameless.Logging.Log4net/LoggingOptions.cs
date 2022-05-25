namespace Nameless.Logging.Log4net {

	public sealed class LoggingOptions {

		#region Public Static Read-Only Fields

		public static readonly LoggingOptions Default = new();

        #endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the configuration file name.
		/// </summary>
		public string ConfigurationFileName { get; set; } = "log4net.config";
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