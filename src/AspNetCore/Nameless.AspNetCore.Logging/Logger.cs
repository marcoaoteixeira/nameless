using Nameless.Logging;
using MS_EventId = Microsoft.Extensions.Logging.EventId;
using MS_ILogger = Microsoft.Extensions.Logging.ILogger;
using MS_LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Nameless.AspNetCore.Logging {

    public sealed class Logger : MS_ILogger {

        #region Private Read-Only Fields

        private readonly ILogger _logger;

        #endregion

        #region Public Constructors

        public Logger(ILogger logger) {
            _logger = logger ?? NullLogger.Instance;
        }

        #endregion

        #region Private Static Methods

        private static LogLevel Parse(MS_LogLevel logLevel) {
            return logLevel switch {
                MS_LogLevel.None => LogLevel.Disabled,
                MS_LogLevel.Debug => LogLevel.Debug,
                MS_LogLevel.Information => LogLevel.Information,
                MS_LogLevel.Warning => LogLevel.Warning,
                MS_LogLevel.Error => LogLevel.Error,
                MS_LogLevel.Critical => LogLevel.Fatal,
                MS_LogLevel.Trace => LogLevel.Audit,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };
        }

        #endregion

        #region MS_ILogger Members

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(MS_LogLevel logLevel) {
            return logLevel switch {
                MS_LogLevel.Debug => _logger.IsEnabled(LogLevel.Debug),
                MS_LogLevel.Information => _logger.IsEnabled(LogLevel.Information),
                MS_LogLevel.Warning => _logger.IsEnabled(LogLevel.Warning),
                MS_LogLevel.Error => _logger.IsEnabled(LogLevel.Error),
                MS_LogLevel.Critical => _logger.IsEnabled(LogLevel.Fatal),
                MS_LogLevel.Trace => _logger.IsEnabled(LogLevel.Audit),
                MS_LogLevel.None => false,
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel)),
            };
        }

        public void Log<TState>(MS_LogLevel logLevel, MS_EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            if (!IsEnabled(logLevel)) { return; }

            _logger.Log(
                logLevel: Parse(logLevel),
                exception: exception,
                message: formatter(state, exception)
            );
        }

        #endregion
    }
}
