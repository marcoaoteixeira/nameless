namespace Nameless.Lucene {

    /// <summary>
    /// Defines methods for search hit.
    /// </summary>
    public interface ISearchHit {

        #region Properties

        /// <summary>
        /// Gets the document item ID.
        /// </summary>
        string DocumentID { get; }

        /// <summary>
        /// Gets the score.
        /// </summary>
        float Score { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the <see cref="int"/> value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The <see cref="int"/> value.</returns>
        int GetInt(string fieldName);

        /// <summary>
        /// Retrieves the <see cref="double"/> value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The <see cref="double"/> value.</returns>
        double GetDouble(string fieldName);

        /// <summary>
        /// Retrieves the <see cref="bool"/> value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The <see cref="bool"/> value.</returns>
        bool GetBoolean(string fieldName);

        /// <summary>
        /// Retrieves the <see cref="string"/> value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The <see cref="string"/> value.</returns>
        string GetString(string fieldName);

        /// <summary>
        /// Retrieves the <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The <see cref="DateTime"/> value.</returns>
        DateTimeOffset GetDateTimeOffset(string fieldName);

        #endregion
    }
}
