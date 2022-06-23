using System.Runtime.Serialization;

namespace Nameless.AspNetCore.Identity.NHibernate {


    [Serializable]
    public class EntityNotFoundException : Exception {

        #region Public Constructors

        public EntityNotFoundException(object id) : this($"Entity with ID {id} was not found.") { }
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}
