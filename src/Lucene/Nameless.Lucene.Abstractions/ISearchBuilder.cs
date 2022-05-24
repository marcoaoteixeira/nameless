namespace Nameless.Lucene {

    /// <summary>
    /// Defines methods for a search builder.
    /// </summary>
    public interface ISearchBuilder {

        #region Methods

        /// <summary>
        /// Parses a query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="escape">Use escape.</param>
        /// <param name="defaultFields">An array of default fields.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder Parse(string query, bool escape = true, params string[] defaultFields);

        /// <summary>
        /// Adds the specified field and value to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithField(string fieldName, bool value);

        /// <summary>
        /// Adds the specified field and value to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithField(string fieldName, DateTime value);

        /// <summary>
        /// Adds the specified field and value to the search.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="useWildcard">Use wild card search.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithField(string field, string value, bool useWildcard);

        /// <summary>
        /// Adds the specified field and value to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithField(string fieldName, int value);

        /// <summary>
        /// Adds the specified field and value to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithField(string fieldName, double value);

        /// <summary>
        /// Adds the specified field and value range to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="minimun">The minimun value.</param>
        /// <param name="maximun">The maximun value.</param>
        /// <param name="includeMinimun">Should include minimun value.</param>
        /// <param name="includeMaximun">Should include maximun value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithinRange(string fieldName, int? minimun, int? maximun, bool includeMinimun = true, bool includeMaximun = true);

        /// <summary>
        /// Adds the specified field and value range to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="minimun">The minimun value.</param>
        /// <param name="maximun">The maximun value.</param>
        /// <param name="includeMinimun">Should include minimun value.</param>
        /// <param name="includeMaximun">Should include maximun value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithinRange(string fieldName, double? minimun, double? maximun, bool includeMinimun = true, bool includeMaximun = true);

        /// <summary>
        /// Adds the specified field and value range to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="minimun">The minimun value.</param>
        /// <param name="maximun">The maximun value.</param>
        /// <param name="includeMinimun">Should include minimun value.</param>
        /// <param name="includeMaximun">Should include maximun value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithinRange(string fieldName, DateTime? minimun, DateTime? maximun, bool includeMinimun = true, bool includeMaximun = true);

        /// <summary>
        /// Adds the specified field and value range to the search.
        /// </summary>
        /// <param name="fieldName">The field.</param>
        /// <param name="minimun">The minimun value.</param>
        /// <param name="maximun">The maximun value.</param>
        /// <param name="includeMinimun">Should include minimun value.</param>
        /// <param name="includeMaximun">Should include maximun value.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder WithinRange(string fieldName, string minimun, string maximun, bool includeMinimun = true, bool includeMaximun = true);

        /// <summary>
        /// Mark a clause as a mandatory match. By default all clauses are optional.
        /// </summary>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder Mandatory();

        /// <summary>
        /// Mark a clause as a forbidden match.
        /// </summary>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder Forbidden();

        /// <summary>
        /// Applied on string clauses, the searched value will not be tokenized.
        /// </summary>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder NotAnalyzed();

        /// <summary>
        /// Applied on string clauses, it removes the default Prefix mecanism. Like 'broadcast' won't
        /// return 'broadcasting'.
        /// </summary>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder ExactMatch();

        /// <summary>
        /// Apply a specific boost to a clause.
        /// </summary>
        /// <param name="weight">A value greater than zero, by which the score will be multiplied.
        /// If greater than 1, it will improve the weight of a clause</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder Weighted(float weight);

        /// <summary>
        /// Defines a clause as a filter, so that it only affect the results of the other clauses.
        /// For instance, if the other clauses returns nothing, even if this filter has matches the
        /// end result will be empty. It's like a two-pass query
        /// </summary>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder AsFilter();

        /// <summary>
        /// Sort by field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder SortBy(string fieldName);

        /// <summary>
        /// Sort by field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder SortByInteger(string fieldName);

        /// <summary>
        /// Sort by field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder SortByBoolean(string fieldName);

        /// <summary>
        /// Sort by field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder SortByString(string fieldName);

        /// <summary>
        /// Sort by field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder SortByDouble(string fieldName);

        /// <summary>
        /// Sort by field.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder SortByDateTime(string fieldName);

        /// <summary>
        /// Order ascending.
        /// </summary>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder Ascending();

        /// <summary>
        /// Slice the search.
        /// </summary>
        /// <param name="skip">Records to skip.</param>
        /// <param name="count">Total of records to retrieve.</param>
        /// <returns>The current instance of <see cref="ISearchBuilder"/>.</returns>
        ISearchBuilder Slice(int skip, int count);

        /// <summary>
        /// Executes search.
        /// </summary>
        /// <returns>A collection of search hits.</returns>
        IEnumerable<ISearchHit> Search();

        /// <summary>
        /// Retrieves the document by its ID.
        /// </summary>
        /// <param name="documentID">The document ID.</param>
        /// <returns>The document.</returns>
        ISearchHit GetDocument(Guid documentID);

        /// <summary>
        /// Retrieves the search bits.
        /// </summary>
        /// <returns>The search bits.</returns>
        ISearchBit GetBits();

        /// <summary>
        /// Returns the count of records for the search.
        /// </summary>
        /// <returns>An integer representing the number of records that the search will retrieve.</returns>
        int Count();

        #endregion
    }
}
