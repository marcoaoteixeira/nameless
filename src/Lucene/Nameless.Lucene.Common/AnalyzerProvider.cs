using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace Nameless.Lucene {

    /// <summary>
    /// Default implementation of <see cref="IAnalyzerProvider"/>.
    /// </summary>
    public sealed class AnalyzerProvider : IAnalyzerProvider {

		#region Private Read-Only Fields

		private readonly IEnumerable<IAnalyzerSelector> _selectors;

		#endregion

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="AnalyzerProvider"/>.
		/// </summary>
		/// <param name="selectors">A collection of <see cref="IAnalyzerSelector"/>.</param>
		public AnalyzerProvider(IEnumerable<IAnalyzerSelector> selectors) {
			_selectors = selectors ?? throw new ArgumentNullException(nameof(selectors));
		}

		#endregion

		#region ILuceneAnalyzerProvider

		/// <inheritdoc />
		public Analyzer GetAnalyzer(string indexName) {
			var analyzer = _selectors
				.Select(_ => _.GetAnalyzer(indexName))
				.Where(_ => _ != null)
				.OrderByDescending(_ => _.Priority)
				.Select(_ => _.Analyzer)
				.FirstOrDefault();

			return analyzer ?? new StandardAnalyzer(IndexProvider.Version);
		}

		#endregion
	}
}
