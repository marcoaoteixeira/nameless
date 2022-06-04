using System.Runtime.Serialization;

namespace Nameless.Persistence.NHibernate {

    [Serializable]
    public class IDNotFoundException : Exception {

        #region Public Constructors

        public IDNotFoundException() { }
        public IDNotFoundException(Type type) : this($"Could not found ID in entity. (Type: {type.AssemblyQualifiedName})") { }
        public IDNotFoundException(string message) : base(message) { }
        public IDNotFoundException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected IDNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}
