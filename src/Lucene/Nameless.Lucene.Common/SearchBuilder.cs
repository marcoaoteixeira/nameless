using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace Nameless.Lucene {

    /// <summary>
    /// Default implementation of <see cref="ISearchBuilder"/>.
    /// </summary>
    public sealed class SearchBuilder : ISearchBuilder {

        #region Private Constants

        private const double EPSILON = 0.001;
        private const int MAX_RESULTS = short.MaxValue;

        #endregion

        #region Private Read-Only Fields

        private readonly Analyzer _analyzer;
        private readonly Func<IndexSearcher> _indexSearcherFactory;
        private readonly IList<BooleanClause> _clauses = new List<BooleanClause>();
        private readonly IList<BooleanClause> _filters = new List<BooleanClause>();

        #endregion

        #region Private Fields

        private bool _asFilter;
        private bool _sortDescending;
        private int _count;
        private int _skip;
        private SortFieldType _comparer;
        private string _sort;

        // pending clause attributes
        private Occur _occur;

        private bool _exactMatch;
        private bool _notAnalyzed;
        private float _boost;
        private Query _query;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SearchBuilder"/>.
        /// </summary>
        /// <param name="indexSearcherFactory">The indexes directory factory.</param>
        /// <param name="analyzer">The analyzer provider.</param>
        public SearchBuilder(Analyzer analyzer, Func<IndexSearcher> indexSearcherFactory) {
            Prevent.ParameterNull(analyzer, nameof(analyzer));
            Prevent.ParameterNull(indexSearcherFactory, nameof(indexSearcherFactory));

            _analyzer = analyzer;
            _indexSearcherFactory = indexSearcherFactory;

            _count = MAX_RESULTS;
            _skip = 0;
            _sort = string.Empty;
            _comparer = 0;
            _sortDescending = true;

            InitializePendingClause();
        }

        #endregion

        #region Private Static Methods

        private static List<string> AnalyzeText(Analyzer analyzer, string field, string text) {
            var result = new List<string>();

            if (string.IsNullOrEmpty(text)) {
                return result;
            }

            using var stringReader = new StringReader(text);
            using var tokenStream = analyzer.GetTokenStream(field, stringReader);
            tokenStream.Reset();
            while (tokenStream.IncrementToken()) {
                try {
                    var attr = tokenStream.GetAttribute<ICharTermAttribute>();
                    if (attr != null) {
                        result.Add(attr.ToString());
                    }
                } catch { }
            }

            return result;
        }

        #endregion

        #region Private Methods

        private void InitializePendingClause() {
            _occur = Occur.SHOULD;
            _exactMatch = false;
            _notAnalyzed = false;
            _query = null;
            _boost = 0;
            _asFilter = false;
        }

        private void CreatePendingClause() {
            if (_query == null) { return; }

            // comparing floating-point numbers using an epsilon value
            if (Math.Abs(_boost - 0) > EPSILON) { _query.Boost = _boost; }

            if (!_notAnalyzed) {
                if (_query is TermQuery termQuery) {
                    var term = termQuery.Term;
                    var analyzedText = AnalyzeText(_analyzer, term.Field, term.Text).FirstOrDefault();
                    _query = new TermQuery(new Term(term.Field, analyzedText));
                }

                if (_query is TermRangeQuery termRangeQuery) {
                    var lowerTerm = AnalyzeText(_analyzer, termRangeQuery.Field, termRangeQuery.LowerTerm.Utf8ToString()).FirstOrDefault();
                    var upperTerm = AnalyzeText(_analyzer, termRangeQuery.Field, termRangeQuery.UpperTerm.Utf8ToString()).FirstOrDefault();

                    _query = new TermRangeQuery(termRangeQuery.Field, new BytesRef(lowerTerm), new BytesRef(upperTerm), termRangeQuery.IncludesLower, termRangeQuery.IncludesUpper);
                }
            }

            if (!_exactMatch) {
                if (_query is TermQuery termQuery) {
                    var term = termQuery.Term;
                    _query = new PrefixQuery(new Term(term.Field, term.Text));
                }
            }

            if (_asFilter) {
                _filters.Add(new BooleanClause(_query, _occur));
            } else {
                _clauses.Add(new BooleanClause(_query, _occur));
            }

            InitializePendingClause();
        }

        private Query CreateQuery() {
            CreatePendingClause();

            var booleanQuery = new BooleanQuery();
            Query resultQuery = booleanQuery;

            if (_clauses.Count == 0) {
                if (_filters.Count > 0) {
                    // only filters applied => transform to a boolean query
                    foreach (var clause in _filters) {
                        booleanQuery.Add(clause);
                    }

                    resultQuery = booleanQuery;
                } else {
                    // search all documents, without filter or clause
                    resultQuery = new MatchAllDocsQuery();
                }
            } else {
                foreach (var clause in _clauses) {
                    booleanQuery.Add(clause);
                }

                if (_filters.Count > 0) {
                    var filter = new BooleanQuery();
                    foreach (var clause in _filters) {
                        filter.Add(clause);
                    }

                    resultQuery = new FilteredQuery(booleanQuery, new QueryWrapperFilter(filter));
                }
            }

            return resultQuery;
        }

        #endregion

        #region ISearchBuilder Members

        /// <inheritdoc />
        public ISearchBuilder Parse(string query, bool escape = true, params string[] defaultFields) {
            Prevent.ParameterNullOrWhiteSpace(query, nameof(query));
            Prevent.ParameterNull(defaultFields, nameof(defaultFields));

            if (defaultFields.Length == 0) { throw new ArgumentException("Default fields can't be empty."); }
            if (escape) { query = QueryParserBase.Escape(query); }

            foreach (var defaultField in defaultFields) {
                CreatePendingClause();
                _query = new QueryParser(IndexProvider.Version, defaultField, _analyzer).Parse(query);
            }

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithField(string field, bool value) => WithField(field, value ? 1 : 0);

        /// <inheritdoc />
        public ISearchBuilder WithField(string field, DateTime value) {
            CreatePendingClause();

            _query = new TermQuery(new Term(field, DateTools.DateToString(value, DateTools.Resolution.MILLISECOND)));

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithField(string field, string value, bool useWildcard) {
            CreatePendingClause();

            if (!string.IsNullOrWhiteSpace(value)) {
                if (useWildcard) {
                    _query = new WildcardQuery(new Term(field, value));
                } else {
                    _query = new TermQuery(new Term(field, QueryParserBase.Escape(value)));
                }
            }

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithField(string field, int value) {
            CreatePendingClause();

            _query = NumericRangeQuery.NewInt32Range(field, value, value, true, true);

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithField(string field, double value) {
            CreatePendingClause();

            _query = NumericRangeQuery.NewDoubleRange(field, value, value, true, true);

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithinRange(string field, int? minimun, int? maximun, bool includeMinimun = true, bool includeMaximun = true) {
            CreatePendingClause();

            _query = NumericRangeQuery.NewInt32Range(field, minimun, maximun, includeMinimun, includeMaximun);

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithinRange(string field, double? minimun, double? maximun, bool includeMinimun = true, bool includeMaximun = true) {
            CreatePendingClause();

            _query = NumericRangeQuery.NewDoubleRange(field, minimun, maximun, includeMinimun, includeMaximun);

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithinRange(string field, DateTime? minimun, DateTime? maximun, bool includeMinimun = true, bool includeMaximun = true) {
            CreatePendingClause();

            var minimunBytesRef = minimun.HasValue ? new BytesRef(DateTools.DateToString(minimun.Value, DateTools.Resolution.MILLISECOND)) : null;
            var maximunBytesRef = maximun.HasValue ? new BytesRef(DateTools.DateToString(maximun.Value, DateTools.Resolution.MILLISECOND)) : null;

            _query = new TermRangeQuery(field, minimunBytesRef, maximunBytesRef, includeMinimun, includeMaximun);

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder WithinRange(string field, string minimun, string maximun, bool includeMinimun = true, bool includeMaximun = true) {
            CreatePendingClause();

            var minimunBytesRef = minimun != null ? new BytesRef(QueryParserBase.Escape(minimun)) : null;
            var maximunBytesRef = maximun != null ? new BytesRef(QueryParserBase.Escape(maximun)) : null;

            _query = new TermRangeQuery(field, minimunBytesRef, maximunBytesRef, includeMinimun, includeMaximun);

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder Mandatory() {
            _occur = Occur.MUST;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder Forbidden() {
            _occur = Occur.MUST_NOT;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder NotAnalyzed() {
            _notAnalyzed = true;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder ExactMatch() {
            _exactMatch = true;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder Weighted(float weight) {
            _boost = weight;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder AsFilter() {
            _asFilter = true;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder SortBy(string name) {
            _sort = name;
            _comparer = 0;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder SortByInteger(string name) {
            _sort = name;
            _comparer = SortFieldType.INT32;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder SortByBoolean(string name) => SortByInteger(name);

        /// <inheritdoc />
        public ISearchBuilder SortByString(string name) {
            _sort = name;
            _comparer = SortFieldType.STRING;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder SortByDouble(string name) {
            _sort = name;
            _comparer = SortFieldType.DOUBLE;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder SortByDateTime(string name) {
            _sort = name;
            _comparer = SortFieldType.INT64;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder Ascending() {
            _sortDescending = false;

            return this;
        }

        /// <inheritdoc />
        public ISearchBuilder Slice(int skip, int count) {
            _skip = skip >= 0 ? skip : 0;
            _count = count >= 1 ? count : 1;

            return this;
        }

        /// <inheritdoc />
        public IEnumerable<ISearchHit> Search() {
            var query = CreateQuery();

            var sort = !string.IsNullOrEmpty(_sort)
                ? new Sort(new SortField(_sort, _comparer, _sortDescending))
                : Sort.RELEVANCE;
            var collector = TopFieldCollector.Create(
                sort: sort,
                numHits: _count + _skip,
                fillFields: false,
                trackDocScores: true,
                trackMaxScore: false,
                docsScoredInOrder: true);

            var indexSearcher = _indexSearcherFactory();
            indexSearcher.Search(query, collector);

            var results = collector.GetTopDocs().ScoreDocs
                .Skip(_skip)
                .Select(scoreDoc => new SearchHit(indexSearcher.Doc(scoreDoc.Doc), scoreDoc.Score))
                .ToList();

            return results;
        }

        /// <inheritdoc />
        public ISearchHit GetDocument(Guid documentID) {
            var query = new TermQuery(new Term(nameof(ISearchHit.DocumentID), documentID.ToString()));
            var indexSearcher = _indexSearcherFactory();
            var hits = indexSearcher.Search(query, 1);

            return hits.ScoreDocs.Length > 0
                ? new SearchHit(indexSearcher.Doc(hits.ScoreDocs[0].Doc), hits.ScoreDocs[0].Score)
                : null;
        }

        /// <inheritdoc />
        public ISearchBit GetBits() {
            var query = CreateQuery();
            var filter = new QueryWrapperFilter(query);
            var indexSearcher = _indexSearcherFactory();
            var context = (AtomicReaderContext)indexSearcher.IndexReader.Context;
            var bits = filter.GetDocIdSet(context, context.AtomicReader.LiveDocs);
            var documentSetIDInterator = new OpenBitSetDISI(bits.GetIterator(), indexSearcher.IndexReader.MaxDoc);

            return new SearchBit(documentSetIDInterator);
        }

        /// <inheritdoc />
        public int Count() {
            var query = CreateQuery();
            var indexSearcher = _indexSearcherFactory();
            var hits = indexSearcher.Search(query, short.MaxValue);
            var length = hits.ScoreDocs.Length;

            return Math.Min(length - _skip, _count);
        }

        #endregion
    }
}
