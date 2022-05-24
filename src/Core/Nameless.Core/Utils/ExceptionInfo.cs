namespace Nameless {

    /// <summary>
    /// Represents an <see cref="Exception"/> information class.
    /// </summary>
    public sealed class ExceptionInfo {

        #region Public Properties

        /// <summary>
        /// Gets the exception type.
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Gets the exception message.
        /// </summary>
        public string? Message { get; }
        /// <summary>
        /// Gets the exception stack trace.
        /// </summary>
        public string? StackTrace { get; }
        /// <summary>
        /// Gets the inner exception info.
        /// </summary>
        public ExceptionInfo? Inner { get; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ExceptionInfo"/>.
        /// </summary>
        /// <param name="type">The exception type.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The exception stack trace.</param>
        /// <param name="inner">The inner exception, if exists.</param>
        public ExceptionInfo(string type, string message, string? stackTrace = null, ExceptionInfo? inner = null) {
            Ensure.NotNullEmptyOrWhiteSpace(type, nameof(type));

            Type = type;
            Message = message;
            StackTrace = stackTrace;
            Inner = inner;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates a new <see cref="ExceptionInfo"/> based on the <see cref="Exception"/>
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>An instance of <see cref="ExceptionInfo"/>.</returns>
        public static ExceptionInfo? Create(Exception? ex) {
            if (ex == null) { return null; }

            return new ExceptionInfo(ex.GetType().FullName!, ex.Message, ex.StackTrace, Create(ex.InnerException));
        }

        #endregion
    }
}