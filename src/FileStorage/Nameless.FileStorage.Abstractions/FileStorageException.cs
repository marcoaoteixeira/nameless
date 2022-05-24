using System.Runtime.Serialization;

namespace Nameless.FileStorage {

    [Serializable]
    public class FileStorageException : Exception {

        #region Public Constructors

        public FileStorageException() { }
        public FileStorageException(string message) : base(message) { }
        public FileStorageException(string message, Exception inner) : base(message, inner) { }

        #endregion

        #region Protected Constructors

        protected FileStorageException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}