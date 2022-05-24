using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Nameless.Data {

    /// <summary>
    /// Extension methods for <see cref="IDatabase"/>
    /// </summary>
    public static class DatabaseExtension {

        #region Public Static Methods

        public static Task<int> ExecuteNonQueryAsync(this IDatabase self, string commandText, CommandType commandType = CommandType.Text, params Parameter[] parameters) {
            if (self == null) { return Task.FromResult(-1); }

            return Task.FromResult(self.ExecuteNonQuery(commandText, commandType, parameters));
        }

        public static Task<TResult?> ExecuteScalarAsync<TResult>(this IDatabase self, string commandText, CommandType commandType = CommandType.Text, params Parameter[] parameters) {
            if (self == null) { return Task.FromResult<TResult?>(default); }

            return Task.FromResult(self.ExecuteScalar<TResult?>(commandText, commandType, parameters));
        }

        public static IAsyncEnumerable<TResult> ExecuteReaderAsync<TResult>(this IDatabase self, string commandText, Func<IDataRecord, TResult> mapper, CommandType commandType = CommandType.Text, params Parameter[] parameters) {
            if (self == null) { return Array.Empty<TResult>().AsAsyncEnumerable(); }

            return self.ExecuteReader(commandText, mapper, commandType, parameters).AsAsyncEnumerable();
        }

        public static Task<TResult?> ExecuteReaderSingleAsync<TResult>(this IDatabase self, string commandText, Func<IDataRecord, TResult> mapper, CommandType commandType = CommandType.Text, params Parameter[] parameters) {
            if (self == null) { return Task.FromResult<TResult?>(default); }

            return Task.FromResult(self.ExecuteReaderSingle(commandText, mapper, commandType, parameters));
        }

        /// <summary>
        /// Executes a reader query against the data base, and returns only one result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="self">The <see cref="IDatabase"/> instance.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">The command type.</param>
        /// <param name="mapper">The mapper for result projection.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>A <see cref="TResult"/> representing the query execution.</returns>
        public static TResult? ExecuteReaderSingle<TResult>(this IDatabase self, string commandText, Func<IDataRecord, TResult> mapper, CommandType commandType = CommandType.Text, params Parameter[] parameters) {
            if (self == null) { return default; }

            using var enumerator = self.ExecuteReader(commandText, mapper, commandType, parameters).GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : default;
        }

        #endregion
    }
}