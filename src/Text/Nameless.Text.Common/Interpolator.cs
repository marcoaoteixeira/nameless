namespace Nameless.Text {

    /// <summary>
    /// Default implementation of <see cref="IInterpolator"/>. Helps interpolate string and objects.
    /// </summary>
    /// <remarks>
    /// http://haacked.com/archive/2009/01/04/fun-with-named-formats-string-parsing-and-edge-cases.aspx/
    /// </remarks>
    public sealed class Interpolator : IInterpolator {

        #region Private Read-Only Fields

        private readonly IDataBinder _dataBinder;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Interpolator"/>.
        /// </summary>
        /// <param name="dataBinder">An instance of <see cref="IDataBinder"/>.</param>
        public Interpolator(IDataBinder dataBinder) {
            _dataBinder = dataBinder ?? throw new ArgumentNullException(nameof(dataBinder));
        }

        #endregion

        #region IInterpolator Members

        /// <inheritdoc />
        public string Interpolate(string format, object obj) {
            if (format == null || !format.Trim().Any()) { throw new ArgumentException("Parameter cannot be null, empty or white spaces.", nameof(format)); }

            var tokens = ExtractExpressions(format)
                .Select(expression => expression.Eval(obj));

            return string.Join(string.Empty, tokens);
        }

        #endregion

        #region Private Static Methods

        private static int IndexOfExpressionStart(string format, int startIndex) {
            var index = format.IndexOf('{', startIndex);

            if (index == -1) { return index; }

            // peek ahead.
            if (index + 1 >= format.Length) { return index; }

            return format[index + 1] == '{'
                ? IndexOfExpressionStart(format, index + 2)
                : index;
        }

        private static int IndexOfExpressionEnd(string format, int startIndex) {
            var endBraceIndex = format.IndexOf('}', startIndex);

            if (endBraceIndex == -1) { return endBraceIndex; }

            // start peeking ahead until there are no more braces...
            // }}}}
            var braceCount = 0;

            for (var idx = endBraceIndex + 1; idx < format.Length; idx++) {
                if (format[idx] == '}') { braceCount++; } else { break; }
            }

            return braceCount % 2 == 1
                ? IndexOfExpressionEnd(format, endBraceIndex + braceCount + 1)
                : endBraceIndex;
        }

        #endregion

        #region Private Methods

        private IEnumerable<TextExpression> ExtractExpressions(string format) {
            var expressionEndIndex = -1;
            int expressionStartIndex;
            do {
                expressionStartIndex = IndexOfExpressionStart(format, expressionEndIndex + 1);

                if (expressionStartIndex < 0) {
                    // everything after last end brace index.
                    if (expressionEndIndex + 1 < format.Length) {
                        yield return new LiteralTextExpression(format[(expressionEndIndex + 1)..]);
                    }

                    break;
                }

                if (expressionStartIndex - expressionEndIndex - 1 > 0) {
                    // everything up to next start brace index
                    yield return new LiteralTextExpression(format.Substring(expressionEndIndex + 1, expressionStartIndex - expressionEndIndex - 1));
                }

                var endBraceIndex = IndexOfExpressionEnd(format, expressionStartIndex + 1);

                if (endBraceIndex > 0) {
                    expressionEndIndex = endBraceIndex;
                    // everything from start to end brace.
                    yield return new FormatTextExpression(_dataBinder, format.Substring(expressionStartIndex, endBraceIndex - expressionStartIndex + 1));
                } else {
                    // rest of string, no end brace (could be invalid expression)
                    yield return new FormatTextExpression(_dataBinder, format[expressionStartIndex..]);
                }
            } while (expressionStartIndex > -1);
        }

        #endregion
    }
}
