namespace Nameless.Logging.NLog {

    public sealed class Logger : ILogger {

        #region Private Static Read-Only Fields

        private static readonly global::NLog.LogLevel AuditLevel = global::NLog.LogLevel.FromString("AUDIT");

        #endregion

        #region Private Read-Only Fields

        private readonly global::NLog.ILogger _logger;

        #endregion

        #region Public Constructors

        public Logger(global::NLog.ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Private Static Methods

        private static global::NLog.LogLevel Parse(LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Disabled => global::NLog.LogLevel.Off,
                LogLevel.Debug => global::NLog.LogLevel.Debug,
                LogLevel.Information => global::NLog.LogLevel.Info,
                LogLevel.Warning => global::NLog.LogLevel.Warn,
                LogLevel.Error => global::NLog.LogLevel.Error,
                LogLevel.Fatal => global::NLog.LogLevel.Fatal,
                LogLevel.Audit => AuditLevel,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };
        }

        #endregion

        #region ILogger Members

        public bool IsEnabled(LogLevel logLevel) {
            return logLevel switch {
                LogLevel.Debug => _logger.IsDebugEnabled,
                LogLevel.Information => _logger.IsInfoEnabled,
                LogLevel.Warning => _logger.IsWarnEnabled,
                LogLevel.Error => _logger.IsErrorEnabled,
                LogLevel.Fatal => _logger.IsFatalEnabled,
                LogLevel.Audit => _logger.IsEnabled(AuditLevel),
                LogLevel.Disabled => false,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };
        }

        public void Log(LogLevel logLevel, Exception? exception, string message, params object[] args) {
            if (IsEnabled(logLevel)) {
                _logger.Log(Parse(logLevel), exception, message, args);
            }
        }

        #endregion
    }
}
