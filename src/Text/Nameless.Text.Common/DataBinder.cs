using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Nameless.Text {

    /// <summary>
    /// Default implementation of <see cref="IDataBinder"/>
    /// </summary>
    public sealed class DataBinder : IDataBinder {

        #region Private Static Read-Only Fields

        private static readonly char[] ExpressionPartSeparator = { '.' };
        private static readonly char[] IndexerExpressionStartChars = { '[', '(' };
        private static readonly char[] IndexerExpressionEndChars = { ']', ')' };

        #endregion

        #region Private Static Methods

        private static object? InnerEval(object container, string expression) {
            var expressionParts = expression.Split(ExpressionPartSeparator);

            return InnerEval(container, expressionParts);
        }

        private static object? InnerEval(object container, string[] expressionParts) {
            object? property;
            int index;

            for (property = container, index = 0; (index < expressionParts.Length) && (property != null); index++) {
                var expressionPart = expressionParts[index];
                var indexerExpression = expressionPart.IndexOfAny(IndexerExpressionStartChars) >= 0;

                property = indexerExpression
                    ? GetIndexedPropertyValue(property, expressionPart)
                    : GetPropertyValue(property, expressionPart);
            }

            return property;
        }

        private static object? GetPropertyValue(object container, string expressionPart) {
            var descriptor = TypeDescriptor
                .GetProperties(container)
                .Find(expressionPart, ignoreCase: false);

            if (descriptor == null) {
                throw new ExpressionPropertyNotFoundException(expressionPart);
            }

            return descriptor.GetValue(container);
        }

        private static object? GetIndexedPropertyValue(object container, string expressionPart) {
            var indexExpressionStart = expressionPart.IndexOfAny(IndexerExpressionStartChars);
            var indexExpressionEnd = expressionPart.IndexOfAny(IndexerExpressionEndChars, indexExpressionStart + 1);

            if ((indexExpressionStart < 0) || (indexExpressionEnd < 0) || (indexExpressionEnd == indexExpressionStart + 1)) {
                throw new ArgumentException("Invalid indexer expression.");
            }

            string? propertyName = null;
            if (indexExpressionStart > 0) {
                propertyName = expressionPart[..indexExpressionStart];
            }

            var hasIndex = false;
            object? indexValue = null;
            var indexToken = expressionPart.Substring(
                startIndex: indexExpressionStart + 1,
                length: indexExpressionEnd - indexExpressionStart - 1
            ).Trim();

            if (indexToken.Length != 0) {
                const string doubleQuote = "\"";
                const string singleQuote = "\'";

                if (!(indexToken.StartsWith(singleQuote) && indexToken.EndsWith(singleQuote)) && !(indexToken.StartsWith(doubleQuote) && indexToken.EndsWith(doubleQuote))) {
                    hasIndex = int.TryParse(indexToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intIndex);

                    if (hasIndex) { indexValue = intIndex; } else { indexValue = indexToken; }
                } else { indexValue = indexToken[1..^1]; }
            }

            if (indexValue == null) {
                throw new ArgumentException("Invalid expression.");
            }

            var collection = !string.IsNullOrWhiteSpace(propertyName)
                ? GetPropertyValue(container, propertyName)
                : container;

            if (collection == null) { return null; }
            if (collection is IList list && hasIndex) {
                return list[(int)indexValue];
            }

            var collectionProperty = collection.GetType()
                .GetTypeInfo()
                .GetProperty(
                    name: "Item",
                    returnType: null,
                    types: new[] { indexValue.GetType() },
                    modifiers: null
                );

            if (collectionProperty == null) {
                throw new IndexerAccessorNotFoundException(collection.GetType().FullName!);
            }

            return collectionProperty.GetValue(collection, new[] { indexValue });
        }

        #endregion

        #region IDataBinder Members

        /// <inheritdoc/>
        public object Eval(object container, string expression, string? format = null) {
            if (container == null) { throw new ArgumentNullException(nameof(container)); }
            if (expression == null || !expression.Trim().Any()) { throw new ArgumentException("Parameter cannot be null, empty or white spaces.", nameof(container)); }

            var value = InnerEval(container, expression);

            if (value == null || value == DBNull.Value) { return string.Empty; }

            return !string.IsNullOrWhiteSpace(format)
                ? string.Format(format, value)
                : value;
        }

        #endregion
    }
}
