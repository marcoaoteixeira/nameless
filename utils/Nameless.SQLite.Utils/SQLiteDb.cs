using System.Data;
using System.Data.SQLite;

namespace Nameless.SQLite.Utils {

    public sealed class SQLiteDb : ISQLiteDb, IDisposable {

        private readonly bool _disposeConnection;

        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of <see cref="SQLiteDb"/>
        /// </summary>
        /// <param name="connection">
        /// The connection.
        /// </param>
        public SQLiteDb(IDbConnection connection) {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _disposeConnection = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SQLiteDb"/>
        /// </summary>
        /// <param name="connectionString">
        /// The connection string. If <c>null</c>, uses "in memory" mode.
        /// </param>
        public SQLiteDb(string? connectionString = null) {
            var connStr = connectionString ?? "Data Source=:memory:;Version=3;Page Size=4096;";

            _connection = new SQLiteConnection(connStr);
            _disposeConnection = true;
        }

        ~SQLiteDb() {
            Dispose(false);
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(SQLiteDb));
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _transaction?.Dispose();

                if (_disposeConnection) {
                    _connection?.Dispose();
                }
            }

            _transaction = null;
            if (_disposeConnection) {
                _connection = null;
            }
            _disposed = true;
        }

        public IDbConnection? Connection => _connection;

        public void BeginTransaction() {
            BlockAccessAfterDispose();

            if (_transaction == null) {
                _transaction = _connection!.BeginTransaction();
            }
        }

        public void CommitTransaction() {
            BlockAccessAfterDispose();

            _transaction?.Commit();
            _transaction?.Dispose();
            _transaction = null;
        }

        public void RollbackTransaction() {
            BlockAccessAfterDispose();

            _transaction?.Rollback();
            _transaction?.Dispose();
            _transaction = null;
        }

        public int ExecuteNonQuery(string sql, params object[] parameters) {
            BlockAccessAfterDispose();

            if (string.IsNullOrWhiteSpace(sql)) { throw new ArgumentException("Argument cannot be null, empty or white spaces.", nameof(sql)); }

            using var command = _connection!.CreateCommand();
            command.CommandText = parameters.Any() ? string.Format(sql, parameters) : sql;
            command.CommandType = CommandType.Text;

            return command.ExecuteNonQuery();
        }

        public object? ExecuteScalar(string sql, params object[] parameters) {
            BlockAccessAfterDispose();

            if (string.IsNullOrWhiteSpace(sql)) { throw new ArgumentException("Argument cannot be null, empty or white spaces.", nameof(sql)); }

            using var command = _connection!.CreateCommand();
            command.CommandText = parameters.Any() ? string.Format(sql, parameters) : sql;
            command.CommandType = CommandType.Text;

            return command.ExecuteScalar();
        }

        public IEnumerable<T> ExecuteReader<T>(string sql, Func<IDataRecord, T> mapper, params object[] parameters) {
            BlockAccessAfterDispose();

            if (string.IsNullOrWhiteSpace(sql)) { throw new ArgumentException("Argument cannot be null, empty or white spaces.", nameof(sql)); }
            if (mapper == null) { throw new ArgumentNullException(nameof(mapper)); }

            using var command = _connection!.CreateCommand();
            command.CommandText = parameters.Any() ? string.Format(sql, parameters) : sql;
            command.CommandType = CommandType.Text;

            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                yield return mapper(reader);
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}