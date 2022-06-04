using System.Runtime.Serialization;

namespace Nameless.Persistence.NHibernate {

    [Serializable]
    public class MultipleEntitiesFoundException : Exception {

        #region Public Constructors

        public MultipleEntitiesFoundException() { }
        public MultipleEntitiesFoundException(string message) : base(message) { }
        public MultipleEntitiesFoundException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected MultipleEntitiesFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}
