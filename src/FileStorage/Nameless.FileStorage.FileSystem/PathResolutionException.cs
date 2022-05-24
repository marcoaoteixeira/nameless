using System.Runtime.Serialization;

namespace Nameless.FileStorage.FileSystem {

    [Serializable]
    public class PathResolutionException : Exception {

        #region Public Constructors

        public PathResolutionException() { }
        public PathResolutionException(string message) : base(message) { }
        public PathResolutionException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected PathResolutionException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}