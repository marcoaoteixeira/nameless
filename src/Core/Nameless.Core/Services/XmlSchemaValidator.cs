using System.Xml;
using System.Xml.Schema;

namespace Nameless.Services {

    /// <summary>
    /// Default implementation of <see cref="IXmlSchemaValidator"/>.
    /// </summary>
    public sealed class XmlSchemaValidator : IXmlSchemaValidator, IDisposable {

        #region Private Fields

        private bool _disposed;

        #endregion

        #region Private Properties

        private bool InvalidState { get; set; }

        #endregion

        #region Destructors

        /// <summary>
        /// Destructor.
        /// </summary>
        ~XmlSchemaValidator() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void OnValidating(ValidationEventArgs e) {
            Validating?.Invoke(e);
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) { /* DO NOTHING */ }

            Validating = null;
            _disposed = true;
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e) {
            OnValidating(e);
            InvalidState = true;
        }

        #endregion

        #region IXmlSchemaValidator Members

        /// <inheritdoc />
        public event Action<ValidationEventArgs>? Validating;

        /// <inheritdoc />
        public bool Validate(Stream schema, Stream xml) {
            Prevent.Null(schema, nameof(schema));
            Prevent.Null(xml, nameof(xml));

            InvalidState = false;

            var settings = new XmlReaderSettings();
            var xmlSchema = XmlSchema.Read(stream: schema, validationEventHandler: null);

            settings.Schemas.Add(xmlSchema!);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += ValidationEventHandler!;

            using (var xmlReader = XmlReader.Create(xml, settings)) {
                while (xmlReader.Read()) { /* Do nothing */ }
            }

            schema.Seek(offset: 0, origin: SeekOrigin.Begin);
            xml.Seek(offset: 0, origin: SeekOrigin.Begin);

            return !InvalidState;
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
