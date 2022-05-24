using System.Data;
using MySql.Data.MySqlClient;

namespace Nameless.Data.MySQL {

    public sealed class DbConnectionProvider : IDbConnectionProvider {

        #region Private Read-Only Fields

        private readonly DatabaseOptions _options;

        #endregion

        #region Private Fields

        private IDbConnection? _connection;
        private bool _disposed;

        #endregion

        #region Public Constructors

        public DbConnectionProvider(DatabaseOptions options) {
            Ensure.NotNull(options, nameof(options));

            _options = options;
        }

        #endregion

        #region Destructors

        ~DbConnectionProvider() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                if (_connection != null) {
                    _connection.Dispose();
                }
            }

            _connection = null;
            _disposed = true;
        }

        #endregion

        #region IDbConnectionProvider Members

        public string ProviderName => "MySQL";

        public IDbConnection GetConnection() {
            if (_connection == null) {
                _connection = new MySqlConnection(_options.ConnectionString);
                _connection.Open();
            }
            return _connection;
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