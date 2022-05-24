using System.Xml.Linq;
using System.Xml.XPath;

namespace Nameless {

    /// <summary>
    /// Extension methods for <see cref="XContainer"/>.
    /// </summary>
    public static class XContainerExtension {

        #region Public Static Methods

        /// <summary>
        /// Verifies if the <paramref name="elementName"/> is present into the <paramref name="self"/> <see cref="XContainer"/>.
        /// </summary>
        /// <param name="self">The self <see cref="XContainer"/>.</param>
        /// <param name="elementName">The element name.</param>
        /// <returns><c>true</c> if exists, otherwise, <c>false</c>.</returns>
        public static bool HasElement(this XContainer self, string elementName) {
            Ensure.NotNullEmptyOrWhiteSpace(elementName, nameof(elementName));

            if (self == null) { return false; }

            return self.Element(elementName) != null;
        }

        /// <summary>
        /// Verifies if the <paramref name="elementName"/> with attribute (specified by <paramref name="attributeName"/>) and attribute value (specified by <paramref name="attributeValue"/>) is present into the <paramref name="self"/> <see cref="XContainer"/>.
        /// </summary>
        /// <param name="self"></param>
        /// <param name="elementName"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeValue"></param>
        /// <returns></returns>
        public static bool HasElement(this XContainer self, string elementName, string attributeName, string attributeValue) {
            Ensure.NotNullEmptyOrWhiteSpace(elementName, nameof(elementName));
            Ensure.NotNullEmptyOrWhiteSpace(attributeName, nameof(attributeName));
            Ensure.NotNullEmptyOrWhiteSpace(attributeValue, nameof(attributeValue));

            if (self == null) { return false; }

            const string expressionPattern = "./{0}[@{1}='{2}']";

            var expression = string.Format(expressionPattern, elementName, attributeName, attributeValue);

            return self.XPathSelectElement(expression) != null;
        }

        #endregion
    }
}