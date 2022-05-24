using MongoDB.Driver;

namespace Nameless.NoSQL.MongoDb {

    public interface IContext {

        #region Methods

        IMongoCollection<T> GetCollection<T>(string? name = null);

        #endregion
    }
}
