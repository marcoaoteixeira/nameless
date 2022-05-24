namespace Nameless.Text {

    /// <summary>
    /// Literal text implementation of <see cref="TextExpression"/>.
    /// </summary>
    public sealed class LiteralTextExpression : TextExpression {

        #region	Private Read-Only Fields

        private readonly string _literalText;

        #endregion

        #region	Public Properties

        /// <summary>
        /// Gets the literal text.
        /// </summary>
        public string LiteralText => _literalText;

        #endregion

        #region	Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="LiteralTextExpression"/>.
        /// </summary>
        /// <param name="literalText">The literal text.</param>
        public LiteralTextExpression(string? literalText = null) {
            _literalText = literalText ?? string.Empty;
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override string? Eval(object? obj) => LiteralText.Replace("{{", "{").Replace("}}", "}");

        #endregion
    }
}
