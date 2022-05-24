namespace Nameless {

    /// <summary>
    /// Extension methods for <see cref="char"/>.
    /// </summary>
    public static class CharExtension {

        #region Public Static Methods

        /// <summary>
        /// Checks if the <see cref="char"/> is an ASCII letter between 'A' and 'Z', or 'a' and 'z'.
        /// </summary>
        /// <param name="self">The source <see cref="char"/>.</param>
        /// <returns><c>true</c> if is a letter, otherwise, <c>false</c>.</returns>
        public static bool IsLetter(this char self) {
            return 'A' <= self && self <= 'Z' || 'a' <= self && self <= 'z';
        }

        /// <summary>
        /// Checks if the <see cref="char"/> is an ASCII carriage return, new line, tab, form feed or space.
        /// </summary>
        /// <param name="self">The source <see cref="char"/>.</param>
        /// <returns><c>true</c> if is a space, otherwise, <c>false</c>.</returns>
        public static bool IsBlank(this char self) {
            return self == '\r' || self == '\n' || self == '\t' || self == '\f' || self == ' ';
        }

        #endregion Public Static Methods
    }
}