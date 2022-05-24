namespace Nameless.Text {

    /// <summary>
    /// Exception for expression property not found.
    /// </summary>
    public class ExpressionPropertyNotFoundException : Exception {

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ExpressionPropertyNotFoundException"/>.
        /// </summary>
        public ExpressionPropertyNotFoundException()
            : base("The given expression could not match the property you are looking for.") { }

        /// <summary>
        /// Initializes a new instance of <see cref="ExpressionPropertyNotFoundException"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
		public ExpressionPropertyNotFoundException(string expression)
            : base($"The given expression could not match the property you are looking for. Expression: {expression}") { }

        /// <summary>
        /// Initializes a new instance of <see cref="ExpressionPropertyNotFoundException"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="inner">The inner exception, if exists.</param>
		public ExpressionPropertyNotFoundException(string expression, Exception inner)
            : base($"The given expression could not match the property you are looking for. Expression: {expression}", inner) { }

        #endregion
    }
}
