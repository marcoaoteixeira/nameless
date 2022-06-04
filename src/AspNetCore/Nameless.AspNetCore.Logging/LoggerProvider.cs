using Nameless.Logging;
using MS_ILogger = Microsoft.Extensions.Logging.ILogger;
using MS_ILoggerProvider = Microsoft.Extensions.Logging.ILoggerProvider;

namespace Nameless.AspNetCore.Logging {

    public sealed class LoggerProvider : MS_ILoggerProvider {

        #region Private Read-Only Fields

        private readonly ILoggerFactory _loggerFactory;

        #endregion

        #region Public Constructors

        public LoggerProvider(ILoggerFactory loggerFactory) {
            _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        }

        #endregion

        #region MS_ILoggerProvider Members

        public MS_ILogger CreateLogger(string categoryName) {
            var type = Type.GetType(categoryName);
            var logger = type != null
                ? _loggerFactory.CreateLogger(type)
                : NullLogger.Instance;

            return new Logger(logger);
        }

        public void Dispose() { } 

        #endregion
    }
}
