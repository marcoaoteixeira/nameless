using System;
using System.Data;

namespace Nameless.Data {

	public interface IDbConnectionProvider : IDisposable {

		#region Properties

		string ProviderName { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Retrieves a database connection.
		/// </summary>
		/// <returns>An instance of <see cref="IDbConnection" />.</returns>
		IDbConnection GetConnection();

		#endregion
	}
}