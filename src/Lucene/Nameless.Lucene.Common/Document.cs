namespace Nameless.Lucene {

    /// <summary>
    /// Default implementation of <see cref="IDocument"/>.
    /// </summary>
    public sealed class Document : IDocument {

        #region Private Read-Only Fields

        private readonly IDictionary<string, DocumentEntry> _entries = new Dictionary<string, DocumentEntry>(StringComparer.CurrentCultureIgnoreCase);

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the document index entries.
        /// </summary>
        public IEnumerable<KeyValuePair<string, DocumentEntry>> Entries {
            get { return _entries; }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Document"/>
        /// </summary>
        /// <param name="id">The document ID.</param>
        public Document(string id) {
            SetID(id);
        }

        #endregion

        #region Private Methods

        private IDocument InnerSet(IndexableType type, string name, object value, DocumentOptions options) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Parameter cannot be null, empty or white spaces.", nameof(name));
            }

            _entries[name] = new DocumentEntry(
               value: value,
               type: type,
               options: options);

            return this;
        }

        #endregion

        #region Public Inner Classes

        /// <summary>
        /// Enumerator for indexable types.
        /// </summary>
        public enum IndexableType {
            /// <summary>
            /// Integers
            /// </summary>
            Integer,
            /// <summary>
            /// String
            /// </summary>
            Text,
            /// <summary>
            /// Date/Time
            /// </summary>
            DateTime,
            /// <summary>
            /// Boolean
            /// </summary>
            Boolean,
            /// <summary>
            /// Float point or decimal numbers.
            /// </summary>
            Number
        }

        /// <summary>
        /// Document index entry.
        /// </summary>
        public class DocumentEntry {

            #region Public Properties

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public object Value { get; set; }
            /// <summary>
            /// Gets or sets the indexable type.
            /// </summary>
            public IndexableType Type { get; set; }
            /// <summary>
            /// Gets or sets the document index options.
            /// </summary>
            public DocumentOptions Options { get; set; }

            #endregion

            #region Public Constructors

            /// <summary>
            /// Initializes a new instance of <see cref="DocumentEntry"/>
            /// </summary>
            /// <param name="value">The value.</param>
            /// <param name="type">The indexable type.</param>
            /// <param name="options">The options.</param>
            public DocumentEntry(object value, IndexableType type, DocumentOptions options) {
                Value = value;
                Type = type;
                Options = options;
            }

            #endregion
        }

        #endregion

        #region IDocumentIndex Members

        /// <summary>
        /// Gets the document ID.
        /// </summary>
        public string? DocumentID { get; private set; }

        /// <inheritdoc />
        public IDocument SetID(string id) {
            DocumentID = id;

            _entries[nameof(ISearchHit.DocumentID)] = new DocumentEntry(
                value: id,
                type: IndexableType.Text,
                options: DocumentOptions.Store);

            return this;
        }

        /// <inheritdoc />
        public IDocument Set(string name, string value, DocumentOptions options) => InnerSet(IndexableType.Text, name, value, options);

        /// <inheritdoc />
        public IDocument Set(string name, DateTimeOffset value, DocumentOptions options) => InnerSet(IndexableType.DateTime, name, value, options);

        /// <inheritdoc />
        public IDocument Set(string name, int value, DocumentOptions options) => InnerSet(IndexableType.Integer, name, value, options);

        /// <inheritdoc />
        public IDocument Set(string name, bool value, DocumentOptions options) => InnerSet(IndexableType.Boolean, name, value, options);

        /// <inheritdoc />
        public IDocument Set(string name, double value, DocumentOptions options) => InnerSet(IndexableType.Number, name, value, options);

        #endregion
    }
}
