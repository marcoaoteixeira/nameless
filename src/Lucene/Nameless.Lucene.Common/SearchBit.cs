using Lucene.Net.Util;

namespace Nameless.Lucene {

    /// <summary>
    /// Default implementation of <see cref="ISearchBit"/>.
    /// </summary>
    public sealed class SearchBit : ISearchBit {

        #region Private Read-Only Fields

        private readonly OpenBitSet _openBitSet;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SearchBit"/>.
        /// </summary>
        /// <param name="openBitSet">The open bit set.</param>
        public SearchBit(OpenBitSet openBitSet) {
            _openBitSet = openBitSet ?? throw new ArgumentNullException(nameof(openBitSet));
        }

        #endregion

        #region Private Methods

        private ISearchBit Apply(ISearchBit other, Action<OpenBitSet, OpenBitSet> operation) {
            var bitset = (OpenBitSet)_openBitSet.Clone();

            if (other is not SearchBit otherBitSet) {
                throw new InvalidOperationException("The other bitset must be of type OpenBitSet");
            }

            operation(bitset, otherBitSet._openBitSet);

            return new SearchBit(bitset);
        }

        #endregion

        #region ISearchBits Members

        /// <inheritdoc />
        public ISearchBit And(ISearchBit other) => Apply(other, (left, right) => left.And(right));

        /// <inheritdoc />
        public ISearchBit Or(ISearchBit other) => Apply(other, (left, right) => left.Or(right));

        /// <inheritdoc />
        public ISearchBit Xor(ISearchBit other) => Apply(other, (left, right) => left.Xor(right));

        /// <inheritdoc />
        public long Count() => _openBitSet.Cardinality;

        #endregion
    }
}
