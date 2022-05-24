using System.Runtime.Serialization;

namespace Nameless {

    /// <summary>
    /// Represents a fatal exception.
    /// </summary>
    [Serializable]
	public class FatalException : Exception {

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="FatalException" />
		/// </summary>
		public FatalException() { }

		/// <summary>
		/// Initializes a new instance of <see cref="FatalException" />
		/// </summary>
		/// <param name="message">The exception message.</param>
		public FatalException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of <see cref="FatalException" />
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="inner">The inner exception.</param>
		public FatalException(string message, Exception inner) : base(message, inner) { }

		#endregion

		#region Protected Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="FatalException" />
		/// </summary>
		/// <param name="info">The serialization info.</param>
		/// <param name="context">The streaming context.</param>
		protected FatalException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		#endregion
	}
}