using MongoDB.Driver;

namespace Nameless.NoSQL.MongoDb {

    public sealed class Context : IContext {

        #region Private Read-Only Fields

        private readonly IMongoDatabase _database;
        private readonly ICollectionNamingStrategy _collectionNamingStrategy;
        private readonly MongoCollectionSettings _collectionSettings;

        #endregion

        #region Public Constructors

        public Context(IMongoDatabase database, ICollectionNamingStrategy? collectionNamingStrategy = null, MongoCollectionSettings? collectionSettings = null) {
            Ensure.NotNull(database, nameof(database));

            _database = database;
            _collectionNamingStrategy = collectionNamingStrategy ?? CollectionNamingStrategy.Instance;
            _collectionSettings = collectionSettings ?? new();
        }

        #endregion

        #region IMongoDbContext Members

        /// <summary>
        /// Retrieves the specified collection.
        /// </summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="name">
        /// If the <paramref name="name"/> is not defined, then the collection
        /// naming strategy will take care of it.
        /// </param>
        /// <returns>The collection.</returns>
        public IMongoCollection<T> GetCollection<T>(string? name = null) {
            var collectionName = string.IsNullOrWhiteSpace(name)
                ? _collectionNamingStrategy.GetCollectionName<T>()
                : name;

            return _database.GetCollection<T>(collectionName, _collectionSettings);
        }

        #endregion
    }
}
