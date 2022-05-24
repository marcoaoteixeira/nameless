using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene_Directory = Lucene.Net.Store.Directory;
using Lucene_Document = Lucene.Net.Documents.Document;
using Lucene_FSDirectory = Lucene.Net.Store.FSDirectory;

namespace Nameless.Lucene {

    /// <summary>
    /// Default implementation of <see cref="IIndex"/>
    /// </summary>
    public sealed class Index : IIndex, IDisposable {

        #region Private Constants

        private const string DATE_PATTERN = "yyyy-MM-ddTHH:mm:ssZ";

        #endregion

        #region Private Read-Only Fields

        private readonly Analyzer _analyzer;
        private readonly string _basePath;
        private readonly string _name;

        private readonly object _syncLock = new object();

        #endregion

        #region Private Fields

        private Lucene_Directory? _directory;
        private IndexReader? _indexReader;
        private IndexSearcher? _indexSearcher;
        private bool _disposed;

        #endregion

        #region Public Static Read-Only Fields

        /// <summary>
        /// GEts the default minimun date time.
        /// </summary>
        public static readonly DateTime DefaultMinDateTime = new(1980, 1, 1);

        /// <summary>
        /// Gets the batch size.
        /// </summary>
        public static readonly int BatchSize = BooleanQuery.MaxClauseCount;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Index"/>.
        /// </summary>
        /// <param name="analyzer">The Lucene analyzer.</param>
        /// <param name="basePath">The base path of the Lucene directory.</param>
        /// <param name="name">The index name.</param>
        public Index(Analyzer analyzer, string basePath, string name) {
            _analyzer = analyzer ?? throw new ArgumentNullException(nameof(analyzer));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
            _name = name ?? throw new ArgumentNullException(nameof(name));

            Initialize();
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Destructor
        /// </summary>
        ~Index() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Static Methods

        private static Lucene_Document CreateDocument(IDocument document) {
            var documentImpl = document as Document;
            var luceneDocument = new Lucene_Document();
            foreach (var entry in documentImpl.Entries) {
                if (entry.Value.Value == null) { continue; }
                var fieldName = entry.Key;
                var fieldValue = entry.Value.Value;

                var store = entry.Value.Options.HasFlag(DocumentOptions.Store) ? Field.Store.YES : Field.Store.NO;
                var analyze = entry.Value.Options.HasFlag(DocumentOptions.Analyze);
                var sanitize = entry.Value.Options.HasFlag(DocumentOptions.Sanitize);

                switch (entry.Value.Type) {
                    case Document.IndexableType.Integer:
                        luceneDocument.Add(new Int32Field(fieldName, Convert.ToInt32(fieldValue), store));
                        break;

                    case Document.IndexableType.Text:
                        var textValue = Convert.ToString(fieldValue);
                        if (sanitize) { textValue = textValue.RemoveHtmlTags(); }
                        if (analyze) { luceneDocument.Add(new TextField(fieldName, textValue, store)); } else { luceneDocument.Add(new StringField(fieldName, textValue, store)); }
                        break;

                    case Document.IndexableType.DateTime:
                        string dateValue;
                        if (fieldValue is DateTimeOffset offset) {
                            dateValue = offset.ToUniversalTime().ToString(DATE_PATTERN);
                        } else {
                            dateValue = ((DateTime)fieldValue).ToUniversalTime().ToString(DATE_PATTERN);
                        }
                        luceneDocument.Add(new StringField(fieldName, dateValue, store));

                        break;

                    case Document.IndexableType.Boolean:
                        luceneDocument.Add(new StringField(fieldName, Convert.ToString(fieldValue).ToLower(), store));
                        break;

                    case Document.IndexableType.Number:
                        luceneDocument.Add(new DoubleField(fieldName, Convert.ToDouble(fieldValue), store));
                        break;

                    default:
                        break;
                }
            }
            return luceneDocument;
        }

        #endregion

        #region Private Methods

        private void Initialize() {
            _directory = Lucene_FSDirectory.Open(new DirectoryInfo(Path.Combine(_basePath, Name)));

            // Creates the index directory
            using (CreateIndexWriter()) { }
        }

        private bool IndexDirectoryExists() => Directory.Exists(Path.Combine(_basePath, Name));

        private IndexWriter CreateIndexWriter() => new(_directory, new IndexWriterConfig(IndexProvider.Version, _analyzer));

        private IndexReader CreateIndexReader() {
            lock (_syncLock) {
                return _indexReader ??= DirectoryReader.Open(_directory);
            }
        }

        private IndexSearcher CreateIndexSearcher() {
            lock (_syncLock) {
                return _indexSearcher ??= new IndexSearcher(CreateIndexReader());
            }
        }

        private void RenewIndex() {
            lock (_syncLock) {
                if (_indexReader != null) {
                    _indexReader.Dispose();
                    _indexReader = null;
                }

                if (_indexSearcher != null) {
                    _indexSearcher = null;
                }
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                if (_directory != null) {
                    _directory.Dispose();
                }

                if (_indexReader != null) {
                    _indexReader.Dispose();
                }
            }

            _directory = null;
            _indexReader = null;
            _indexSearcher = null;
            _disposed = true;
        }

        #endregion

        #region IIndex Members

        /// <inheritdoc />
        public string Name => _name;

        /// <inheritdoc />
        public bool IsEmpty() => TotalDocuments() <= 0;

        /// <inheritdoc />
        public int TotalDocuments() {
            if (!IndexDirectoryExists()) { return -1; }

            return CreateIndexReader().NumDocs;
        }

        /// <inheritdoc />
        public IDocument NewDocument(string documentID) => new Document(documentID);

        /// <inheritdoc />
        public void StoreDocuments(params IDocument[] documents) {
            if (documents == null) { return; }
            if (documents.Length == 0) { return; }

            DeleteDocuments(documents.OfType<Document>().Select(_ => _.DocumentID).ToArray());

            using var writer = CreateIndexWriter();
            foreach (var document in documents) {
                writer.AddDocument(CreateDocument(document));
            }

            RenewIndex();
        }

        /// <inheritdoc />
        public void DeleteDocuments(params string[] documentIDs) {
            if (documentIDs == null) { return; }
            if (documentIDs.Length == 0) { return; }

            using var writer = CreateIndexWriter();
            // Process documents by batch as there is a max number of terms
            // a query can contain (1024 by default).
            var pageCount = (documentIDs.Length / BatchSize) + 1;
            for (var page = 0; page < pageCount; page++) {
                var query = new BooleanQuery();
                try {
                    var batch = documentIDs.Skip(page * BatchSize).Take(BatchSize);
                    foreach (var id in batch) {
                        query.Add(new BooleanClause(new TermQuery(new Term(nameof(ISearchHit.DocumentID), id.ToString())), Occur.SHOULD));
                    }
                    writer.DeleteDocuments(query);
                } catch { /* Just skip error */ }
            }

            RenewIndex();
        }

        /// <inheritdoc />
        public ISearchBuilder CreateSearchBuilder() => new SearchBuilder(_analyzer, CreateIndexSearcher);

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
