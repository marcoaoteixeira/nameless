namespace Nameless.Lucene {

    /// <summary>
    /// Defines methods for an index representation.
    /// </summary>
    public interface IIndex {

        #region Properties

        /// <summary>
        /// Gets the name of the index.
        /// </summary>
        string Name { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Whether an index is empty or not
        /// </summary>
        bool IsEmpty();

        /// <summary>
        /// Gets the number of indexed documents
        /// </summary>
        int TotalDocuments();

        /// <summary>
        /// Creates an empty document
        /// </summary>
        /// <returns></returns>
        IDocument NewDocument(string documentID);

        /// <summary>
        /// Adds a set of new document to the index
        /// </summary>
        void StoreDocuments(params IDocument[] documents);

        /// <summary>
        /// Removes a set of existing document from the index
        /// </summary>
        void DeleteDocuments(params string[] documentIDs);

        /// <summary>
        /// Creates a search builder for this provider
        /// </summary>
        /// <returns>A search builder instance</returns>
        ISearchBuilder CreateSearchBuilder();

        #endregion
    }
}
