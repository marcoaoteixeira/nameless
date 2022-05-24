using Lucene.Net.Analysis;

namespace Nameless.Lucene {

    /// <summary>
    /// Represents a Lucene analyzer selector result.
    /// </summary>
    public sealed class AnalyzerSelectorResult {

        #region Public Properties

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the analyzer instance.
        /// </summary>
        public Analyzer? Analyzer { get; set; }

        #endregion
    }
}
