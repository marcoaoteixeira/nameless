using System.Data;

namespace Nameless.SQLite.Utils {

    public interface ISQLiteDb {

        IDbConnection? Connection { get; }

        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();

        int ExecuteNonQuery(string sql, params object[] parameters);
        object? ExecuteScalar(string sql, params object[] parameters);
        IEnumerable<T> ExecuteReader<T>(string sql, Func<IDataRecord, T> mapper, params object[] parameters);

    }
}
