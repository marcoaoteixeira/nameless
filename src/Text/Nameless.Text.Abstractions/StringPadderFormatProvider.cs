using System;

namespace Nameless.Text {

    /// <summary>
    /// The string padder format provider.
    /// </summary>
    public sealed class StringPadderFormatProvider : IFormatProvider {

        #region Public Static Read-Only Fields

        /// <summary>
        /// Instance of left string padder format provider.
        /// </summary>
        public static readonly StringPadderFormatProvider Left = new(StringPadderDirection.Left);

        /// <summary>
        /// Instance of right string padder format provider.
        /// </summary>
        public static readonly StringPadderFormatProvider Right = new(StringPadderDirection.Right);

        #endregion

        #region Private Read-Only Fields

        private readonly StringPadderDirection _direction;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the string padder direction.
        /// </summary>
        public StringPadderDirection Direction => _direction;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="StringPadderFormatProvider"/>.
        /// </summary>
        /// <param name="direction">The direction.</param>
        public StringPadderFormatProvider(StringPadderDirection direction = StringPadderDirection.Right) {
            _direction = direction;
        }

        #endregion

        #region IFormatProvider Members

        /// <inheritdoc />
        public object? GetFormat(Type? formatType) => formatType == typeof(ICustomFormatter)
            ? new StringPadderFormatter(Direction)
            : null;

        #endregion
    }
}
