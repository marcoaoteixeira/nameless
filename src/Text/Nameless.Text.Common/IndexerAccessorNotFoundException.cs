namespace Nameless.Text {

    /// <summary>
    /// Exception for indexer accessor not found.
    /// </summary>
    public class IndexerAccessorNotFoundException : Exception {

		#region Public Constructors

		/// <summary>
		/// Initializes a new instance of <see cref="IndexerAccessorNotFoundException"/>.
		/// </summary>
		public IndexerAccessorNotFoundException()
			: base("Indexer accessor not found.") { }

		/// <summary>
		/// Initializes a new instance of <see cref="IndexerAccessorNotFoundException"/>.
		/// </summary>
		/// <param name="typeName">The type name.</param>
		public IndexerAccessorNotFoundException(string typeName)
			: base($"Indexer accessor not found. Type: {typeName}") { }

		/// <summary>
		/// Initializes a new instance of <see cref="IndexerAccessorNotFoundException"/>.
		/// </summary>
		/// <param name="typeName">The type name.</param>
		/// <param name="inner">The inner exception, if exists.</param>
		public IndexerAccessorNotFoundException(string typeName, Exception inner)
			: base($"Indexer accessor not found. Type: {typeName}", inner) { }

		#endregion
	}
}
