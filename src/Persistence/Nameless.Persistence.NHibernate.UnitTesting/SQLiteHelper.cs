using System.Data;
using System.Data.SQLite;

namespace Nameless.Persistence.NHibernate.UnitTesting {

    public sealed class SQLiteHelper : IDisposable {

        private readonly IDbConnection _dbConnection;
        private bool _disposed;

        public SQLiteHelper(string? connectionString = null) {
            var currentConnStr = connectionString ?? "Data Source=:memory:;Version=3;Page Size=4096;BinaryGUID=False;";
            _dbConnection = new SQLiteConnection(currentConnStr);
            _dbConnection.Open();
        }

        public int ExecuteNonQuery(string sql, IDictionary<string, object>? parameters = null) {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            using var command = _dbConnection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null && parameters.Any()) {
                foreach (var parameter in parameters) {
                    command.Parameters.Add(new SQLiteParameter(parameter.Key, parameter.Value));
                }
            }

            int result;
            using var transaction = _dbConnection.BeginTransaction();
            try {
                result = command.ExecuteNonQuery();
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return result;
        }

        public object? ExecuteScalar(string sql, IDictionary<string, object>? parameters = null) {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            using var command = _dbConnection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null && parameters.Any()) {
                foreach (var parameter in parameters) {
                    command.Parameters.Add(new SQLiteParameter(parameter.Key, parameter.Value));
                }
            }

            object? result;
            using var transaction = _dbConnection.BeginTransaction();
            try {
                result = command.ExecuteScalar();
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return result;
        }

        public IEnumerable<TResult> ExecuteReader<TResult>(string sql, Func<IDataRecord, TResult> mapper, IDictionary<string, object>? parameters = null) {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }

            using var command = _dbConnection.CreateCommand();
            command.CommandText = sql;

            if (parameters != null && parameters.Any()) {
                foreach (var parameter in parameters) {
                    command.Parameters.Add(new SQLiteParameter(parameter.Key, parameter.Value));
                }
            }

            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                yield return mapper(reader);
            }
        }

        public void Dispose() {
            if (_disposed) { return; }

            _dbConnection.Close();
            _dbConnection.Dispose();
            _disposed = true;
        }

    }
}
