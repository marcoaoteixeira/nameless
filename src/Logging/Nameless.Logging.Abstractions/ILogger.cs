namespace Nameless.Logging {

    /// <summary>
    /// Defines the log interface.
    /// </summary>
    public interface ILogger {

		#region Methods

		/// <summary>
		/// Check if the specified log level is enabled.
		/// </summary>
		/// <param name="logLevel">Log level.</param>
		/// <returns><c>true</c> if log level is enabled, otherwise, <c>false</c>.</returns>
		bool IsEnabled(LogLevel logLevel);

		/// <summary>
		/// Writes the log information.
		/// </summary>
		/// <param name="logLevel">Log level.</param>
		/// <param name="exception">The exception, if any.</param>
		/// <param name="message">The log message.</param>
		/// <param name="args">The arguments for the log message, if any.</param>
		void Log(LogLevel logLevel, Exception? exception, string message, params object[] args);

		#endregion Methods
	}
}