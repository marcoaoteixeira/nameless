using System;
using System.Collections.Generic;
using System.Data;

namespace Nameless.Data {

    /// <summary>
    /// Contract to a database accessor that works with ADO.Net
    /// </summary>
    public interface IDatabase {

		#region Methods

		/// <summary>
		/// Starts a database transaction and returns a reference to the transaction to caller.
		/// If a transaction was already started, returns its reference.
		/// </summary>
		/// <param name="level">The isolation level.</param>
		/// <returns>A reference to the transaction object.</returns>
		IDbTransaction StartTransaction(IsolationLevel level = IsolationLevel.Unspecified);

		/// <summary>
		/// Executes a not-query command against the data base.
		/// </summary>
		/// <param name="commandText">The command text.</param>
		/// <param name="commandType">The command type. Default is <see cref="CommandType.Text"/>.</param>
		/// <param name="parameters">The command parameters.</param>
		/// <returns>An <see cref="int"/> value representing the records affected.</returns>
		int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, params Parameter[] parameters);

		/// <summary>
		/// Executes a reader query against the data base.
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="commandText">The command text.</param>
		/// <param name="commandType">The command type.</param>
		/// <param name="mapper">The mapper for result projection.</param>
		/// <param name="parameters">The command parameters.</param>
		/// <returns>A <see cref="IEnumerable{TResult}"/> implementation instance, representing a collection of results.</returns>
		IEnumerable<TResult> ExecuteReader<TResult>(string commandText, Func<IDataRecord, TResult> mapper, CommandType commandType = CommandType.Text, params Parameter[] parameters);

		/// <summary>
		/// Executes a scalar command against the data base.
		/// </summary>
		/// <param name="commandText">The command text.</param>
		/// <param name="commandType">The command type.</param>
		/// <param name="parameters">The command parameters.</param>
		/// <returns>A <see cref="TResult"/> representing the query result.</returns>
		TResult? ExecuteScalar<TResult>(string commandText, CommandType commandType = CommandType.Text, params Parameter[] parameters);

		#endregion
	}
}