using MongoDB.Bson.Serialization;

namespace Nameless.NoSQL.MongoDb {

    public abstract class ClassMappingBase<TDocument> {

        #region Protected Constructors

        protected ClassMappingBase() {
            Initialize();
        }

        #endregion

        #region Private Methods

        private void Initialize() {
            Map(BsonClassMap.RegisterClassMap<TDocument>());
        }

        #endregion

        #region IClassMapping<TDocument> Members

        public abstract void Map(BsonClassMap<TDocument> mapper);

        #endregion
    }
}
