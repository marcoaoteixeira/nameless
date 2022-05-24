using System.Runtime.Serialization;

namespace Nameless.AspNetCore.Exceptions {

    [Serializable]
    public class AuthenticationException : Exception {

        #region Public Constructors

        public AuthenticationException() : this("Authentication credentials are incorrect.") { }
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected AuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}
