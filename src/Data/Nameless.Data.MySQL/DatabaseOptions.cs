namespace Nameless.Data.MySQL {

	public sealed class DatabaseOptions {

		#region Public Properties

		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		public string ConnectionString { get; set; } = "Server=myServerAddress;Database=myDataBase;Uid=myUsername;Pwd=myPassword;";

		#endregion
	}
}