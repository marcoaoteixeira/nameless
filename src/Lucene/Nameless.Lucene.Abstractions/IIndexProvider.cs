namespace Nameless.Lucene {

    /// <summary>
    /// Defines methods for an index provider.
    /// </summary>
    public interface IIndexProvider {

        #region Methods

        /// <summary>
        /// Deletes an existing index
        /// </summary>
        void Delete(string indexName);

        /// <summary>
        /// Checks whether an index is already existing or not
        /// </summary>
        bool Exists(string indexName);

        /// <summary>
        /// Retrieves an index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>The index.</returns>
        IIndex GetOrCreate(string indexName);

        /// <summary>
        /// Lists all existing indexes
        /// </summary>
        IEnumerable<string> List();

        #endregion
    }
}
