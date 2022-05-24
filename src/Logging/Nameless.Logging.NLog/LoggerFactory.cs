using System.Reflection;

namespace Nameless.Logging.NLog {

    public sealed class LoggerFactory : ILoggerFactory, IDisposable {

        #region Private Read-Only Fields

        private readonly LoggingOptions _options;

        #endregion

        #region Private Fields

        private global::NLog.LogFactory? _factory;
        private bool _disposed;

        #endregion

        #region Public Constructors

        public LoggerFactory(LoggingOptions? options = null) {
            _options = options ?? new();

            Initialize();
        }

        #endregion

        #region Destructor

        ~LoggerFactory() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Static Methods

        private static string GetConfigurationFilePath(string configurationFileName) {
            return Path.IsPathRooted(configurationFileName)
                ? configurationFileName
                : Path.Combine(typeof(LoggerFactory).GetTypeInfo().Assembly.GetDirectoryPath()!, configurationFileName);
        }

        #endregion

        #region Private Methods

        private void Initialize() {
            var configFilePath = GetConfigurationFilePath(_options.ConfigurationFileName);
            var config = new global::NLog.Config.XmlLoggingConfiguration(configFilePath) {
                AutoReload = _options.ReloadOnChange
            };
            _factory = new global::NLog.LogFactory(config);
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _factory!.Shutdown();
                _factory!.Dispose();
            }

            _factory = null;
            _disposed = true;
        }

        #endregion

        #region ILoggerFactory Members

        public ILogger CreateLogger(Type type) {
            BlockAccessAfterDispose();

            if (type == null) { throw new ArgumentNullException(nameof(type)); }

            return new Logger(_factory!.GetLogger(type.FullName));
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
