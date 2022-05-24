using System;

namespace Nameless.NoSQL.MongoDb {

    public interface ICollectionNamingStrategy {

        #region Methods

        string? GetCollectionName(Type? type);

        #endregion
    }

    public static class CollectionNamingStrategyExtension {

        #region Public Static Methods

        public static string? GetCollectionName<T>(this ICollectionNamingStrategy self) {
            if (self == null) { return default; }

            return self.GetCollectionName(typeof(T));
        }

        #endregion
    }
}
