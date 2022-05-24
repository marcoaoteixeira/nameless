namespace Nameless.NoSQL.MongoDb {

    public class MongoDbOptions {

		#region Public Properties

		public string? ConnectionString { get; set; }
		public string? Host { get; set; }
        public int Port { get; set; }
        public string? DatabaseName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }

        #endregion
    }
}
