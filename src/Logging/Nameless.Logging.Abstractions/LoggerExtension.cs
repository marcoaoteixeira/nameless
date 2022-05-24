namespace Nameless.Logging {

    /// <summary>
    /// Extension methods for <see cref="ILogger"/>.
    /// </summary>
    public static class LoggerExtension {

        #region Private Static Read-Only Fields

        private static readonly object[] EmptyArgsArray = Array.Empty<object>();

        #endregion

        #region Public Static Methods

        #region Audit Log Methods

        /// <summary>
        /// Writes an audit log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="message">The message to log.</param>
        public static void Audit(this ILogger source, string message) {
            FilteredLog(source, LogLevel.Audit, null, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes an audit log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Audit(this ILogger source, Exception exception, string message) {
            FilteredLog(source, LogLevel.Audit, exception, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes an audit log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Audit(this ILogger source, string format, params object[] args) {
            FilteredLog(source, LogLevel.Audit, null, format, args);
        }

        /// <summary>
        /// Writes an audit log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Audit(this ILogger source, Exception exception, string format, params object[] args) {
            FilteredLog(source, LogLevel.Audit, exception, format, args);
        }

        #endregion

        #region Debug Log Methods

        /// <summary>
        /// Writes a debug log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="message">The message to log.</param>
        public static void Debug(this ILogger source, string message) {
            FilteredLog(source, LogLevel.Debug, null, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes a debug log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Debug(this ILogger source, Exception exception, string message) {
            FilteredLog(source, LogLevel.Debug, exception, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes a debug log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Debug(this ILogger source, string format, params object[] args) {
            FilteredLog(source, LogLevel.Debug, null, format, args);
        }

        /// <summary>
        /// Writes a debug log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Debug(this ILogger source, Exception exception, string format, params object[] args) {
            FilteredLog(source, LogLevel.Debug, exception, format, args);
        }

        #endregion

        #region Information Log Methods

        /// <summary>
        /// Writes an information log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="message">The message to log.</param>
        public static void Information(this ILogger source, string message) {
            FilteredLog(source, LogLevel.Information, null, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes an information log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Information(this ILogger source, Exception exception, string message) {
            FilteredLog(source, LogLevel.Information, exception, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes an information log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Information(this ILogger source, string format, params object[] args) {
            FilteredLog(source, LogLevel.Information, null, format, args);
        }

        /// <summary>
        /// Writes an information log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Information(this ILogger source, Exception exception, string format, params object[] args) {
            FilteredLog(source, LogLevel.Information, exception, format, args);
        }

        #endregion

        #region Warning Log Methods

        /// <summary>
        /// Writes a warning log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="message">The message to log.</param>
        public static void Warning(this ILogger source, string message) {
            FilteredLog(source, LogLevel.Warning, null, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes a warning log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Warning(this ILogger source, Exception exception, string message) {
            FilteredLog(source, LogLevel.Warning, exception, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes a warning log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Warning(this ILogger source, string format, params object[] args) {
            FilteredLog(source, LogLevel.Warning, null, format, args);
        }

        /// <summary>
        /// Writes a warning log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Warning(this ILogger source, Exception exception, string format, params object[] args) {
            FilteredLog(source, LogLevel.Warning, exception, format, args);
        }

        #endregion

        #region Error Log Methods

        /// <summary>
        /// Writes an error log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="message">The message to log.</param>
        public static void Error(this ILogger source, string message) {
            FilteredLog(source, LogLevel.Error, null, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes an error log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Error(this ILogger source, Exception exception, string message) {
            FilteredLog(source, LogLevel.Error, exception, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes an error log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Error(this ILogger source, string format, params object[] args) {
            FilteredLog(source, LogLevel.Error, null, format, args);
        }

        /// <summary>
        /// Writes an error log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Error(this ILogger source, Exception exception, string format, params object[] args) {
            FilteredLog(source, LogLevel.Error, exception, format, args);
        }

        #endregion

        #region Fatal Log Methods

        /// <summary>
        /// Writes a fatal log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="message">The message to log.</param>
        public static void Fatal(this ILogger source, string message) {
            FilteredLog(source, LogLevel.Fatal, null, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes a fatal log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="message">The message to log.</param>
        public static void Fatal(this ILogger source, Exception exception, string message) {
            FilteredLog(source, LogLevel.Fatal, exception, message, EmptyArgsArray);
        }

        /// <summary>
        /// Writes a fatal log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Fatal(this ILogger source, string format, params object[] args) {
            FilteredLog(source, LogLevel.Fatal, null, format, args);
        }

        /// <summary>
        /// Writes a fatal log line.
        /// </summary>
        /// <param name="source">The source (<see cref="ILogger"/>).</param>
        /// <param name="exception">The <see cref="Exception"/> to log.</param>
        /// <param name="format">The message format to log.</param>
        /// <param name="args">The message format arguments, if any.</param>
        public static void Fatal(this ILogger source, Exception exception, string format, params object[] args) {
            FilteredLog(source, LogLevel.Fatal, exception, format, args);
        }

        #endregion

        #endregion

        #region Private Static Methods

        private static void FilteredLog(ILogger logger, LogLevel level, Exception? exception, string format, object[] objects) {
            if (logger.IsEnabled(level)) {
                logger.Log(level, exception, format, objects);
            }
        }

        #endregion
    }
}