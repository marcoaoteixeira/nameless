using System;

namespace Nameless.NoSQL.MongoDb {

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CollectionNameAttribute : Attribute {

        #region Public Read-Only Properties

        public string Name { get; }

        #endregion

        #region Public Constructors

        public CollectionNameAttribute(string name) {
            Name = name;
        }

        #endregion
    }
}
