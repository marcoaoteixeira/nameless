namespace Nameless.Lucene {

	/// <summary>
	/// Defines methods for a Lucene analyzer selector.
	/// </summary>
	public interface IAnalyzerSelector {

        #region Methods

        /// <summary>
        /// Retrieves a Lucene analyzer selector result.
        /// </summary>
        /// <param name="indexName">The index name.</param>
        /// <returns>An instance of <see cref="AnalyzerSelectorResult"/>.</returns>
        AnalyzerSelectorResult GetAnalyzer(string indexName);

        #endregion
    }
}
