using System.Data;
using System.Data.SqlClient;

namespace Nameless.Data.MSSQLServer {

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
            Prevent.Null(options, nameof(options));

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

        public string ProviderName => "Microsoft SQL Server";

        public IDbConnection GetConnection() {
            if (_connection == null) {
                _connection = new SqlConnection(_options.ConnectionString);
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