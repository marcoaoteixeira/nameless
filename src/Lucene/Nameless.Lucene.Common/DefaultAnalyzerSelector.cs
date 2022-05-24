using Lucene.Net.Analysis.Standard;

namespace Nameless.Lucene {

    /// <summary>
    /// Default implementation of <see cref="IAnalyzerSelector"/>.
    /// </summary>
    public sealed class DefaultAnalyzerSelector : IAnalyzerSelector {

        #region IAnalyzerSelector Members

        /// <inheritdoc />
        public AnalyzerSelectorResult GetAnalyzer(string indexName) {
            return new AnalyzerSelectorResult {
                Priority = -5,
                Analyzer = new StandardAnalyzer(IndexProvider.Version)
            };
        }

        #endregion
    }
}
