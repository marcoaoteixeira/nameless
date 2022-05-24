using System.Globalization;
using log4net;
using log4net.Core;

namespace Nameless.Logging.Log4net {

    /// <summary>
    /// log4net implementation of <see cref="ILogger"/>
    /// </summary>
    public sealed class Logger : ILogger {

        #region Private Static Read-Only Fields

        private static readonly Level AuditLevel = new(2000000000, "AUDIT");

        #endregion

        #region Private Read-Only Fields

        private readonly ILog _log;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initalizes a new instance of <see cref="Logger"/>
        /// </summary>
        /// <param name="log">The log4net log instance</param>
        public Logger(ILog log) {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        #endregion

        #region Private Static Methods

        private static Level Parse(LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Disabled => Level.Off,
                LogLevel.Debug => Level.Debug,
                LogLevel.Information => Level.Info,
                LogLevel.Warning => Level.Warn,
                LogLevel.Error => Level.Error,
                LogLevel.Fatal => Level.Fatal,
                LogLevel.Audit => AuditLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };
        }

        #endregion

        #region ILogger Members

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Debug => _log.IsDebugEnabled,
                LogLevel.Information => _log.IsInfoEnabled,
                LogLevel.Warning => _log.IsWarnEnabled,
                LogLevel.Error => _log.IsErrorEnabled,
                LogLevel.Fatal => _log.IsFatalEnabled,
                LogLevel.Audit => _log.Logger.IsEnabledFor(AuditLevel),
                LogLevel.Disabled => false,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };
        }

        /// <inheritdoc />
        public void Log(LogLevel logLevel, Exception? exception, string message, params object[] args) {
            if (!IsEnabled(logLevel)) { return; }

            var logType = Type.GetType(_log.Logger.Name);
            var level = Parse(logLevel);
            var currentMessage = !args.IsNullOrEmpty() ? string.Format(CultureInfo.CurrentCulture, message, args) : message;

            _log.Logger.Log(logType, level, currentMessage, exception);
        }

        #endregion
    }
}