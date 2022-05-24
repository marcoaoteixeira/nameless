using System.Runtime.Serialization;

namespace Nameless.EventSourcing {

    /// <summary>
    /// Exception for aggregate and event without ID.
    /// </summary>
    [Serializable]
    public class ConcurrencyException : Exception {

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ConcurrencyException"/>
        /// </summary>
        public ConcurrencyException() { }

        /// <summary>
        /// Initializes a new instance of <see cref="ConcurrencyException"/>
        /// </summary>
        /// <param name="message">The exception message</param>
        public ConcurrencyException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of <see cref="ConcurrencyException"/>
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ConcurrencyException" />
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }
}
