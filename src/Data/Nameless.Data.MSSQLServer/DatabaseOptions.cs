namespace Nameless.Data.MSSQLServer {

	public sealed class DatabaseOptions {

		#region Public Properties

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		public string ConnectionString { get; set; } = "Server=.;Database=master;User Id=sa;Password=sa;";

		#endregion
	}
}