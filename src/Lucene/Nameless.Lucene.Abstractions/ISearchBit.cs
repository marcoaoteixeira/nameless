namespace Nameless.Lucene {

    /// <summary>
    /// Defines methods for a search bit class.
    /// </summary>
    public interface ISearchBit {

        #region Methods

        /// <summary>
        /// Operator AND
        /// </summary>
        /// <param name="other">The other search bit.</param>
        /// <returns>The current instance of <see cref="ISearchBit"/>.</returns>
        ISearchBit And(ISearchBit other);

        /// <summary>
        /// Operator OR
        /// </summary>
        /// <param name="other">The other search bit.</param>
        /// <returns>The current instance of <see cref="ISearchBit"/>.</returns>
        ISearchBit Or(ISearchBit other);

        /// <summary>
        /// Operator XOR
        /// </summary>
        /// <param name="other">The other search bit.</param>
        /// <returns>The current instance of <see cref="ISearchBit"/>.</returns>
        ISearchBit Xor(ISearchBit other);

        /// <summary>
        /// Retrieves the count of the search bit cardinality.
        /// </summary>
        /// <returns>A <see cref="long"/> representing the count of cardinality.</returns>
        long Count();

        #endregion
    }
}
