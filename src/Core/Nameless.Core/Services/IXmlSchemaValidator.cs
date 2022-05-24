using System.Xml.Schema;

namespace Nameless.Services {

    /// <summary>
    /// Defines methods for validate a XML via XML schema.
    /// </summary>
    public interface IXmlSchemaValidator {

        #region Events

        /// <summary>
        /// Triggers when validate a XML.
        /// </summary>
        event Action<ValidationEventArgs> Validating;

        #endregion

        #region Methods

        /// <summary>
        /// Validates the XML <see cref="Stream"/>.
        /// </summary>
        /// <param name="schema">The XML schema <see cref="Stream"/>.</param>
        /// <param name="xml">The XML <see cref="Stream"/>.</param>
        /// <returns><c>true</c> if is valid; otherwise, <c>false</c>.</returns>
        bool Validate(Stream schema, Stream xml);

        #endregion
    }
}
