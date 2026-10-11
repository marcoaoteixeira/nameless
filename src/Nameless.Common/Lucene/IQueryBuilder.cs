namespace Nameless.Lucene;

/// <summary>
///     Defines a fluent builder for constructing Lucene search queries,
///     including field filters, range clauses, sorting, and pagination.
/// </summary>
public interface IQueryBuilder {
    /// <summary>
    ///     Includes the specified fields and value to the search query.
    /// </summary>
    /// <param name="names">
    ///     The fields name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <param name="fuzziness">
    ///     The amount of fuzzy, the value must be between 0.0 and 2.0.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithFields(string[] names, string value, float fuzziness);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, bool value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <param name="useWildcard">
    ///     Whether it should Use wild card for search.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, string value, bool useWildcard);

    /// <summary>
    ///     Includes the specified field and values to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="values">
    ///     The values.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, string[] values);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, int value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, long value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, float value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, double value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, DateTimeOffset value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, DateTime value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, DateOnly value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, TimeOnly value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, TimeSpan value);

    /// <summary>
    ///     Includes the specified field and value to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithField(string name, Enum value);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, string? minimum, string? maximum, bool includeMinimum, bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, int? minimum, int? maximum, bool includeMinimum, bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, long? minimum, long? maximum, bool includeMinimum, bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, float? minimum, float? maximum, bool includeMinimum, bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, double? minimum, double? maximum, bool includeMinimum, bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, DateTimeOffset? minimum, DateTimeOffset? maximum, bool includeMinimum,
        bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, DateTime? minimum, DateTime? maximum, bool includeMinimum,
        bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, DateOnly? minimum, DateOnly? maximum, bool includeMinimum,
        bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, TimeOnly? minimum, TimeOnly? maximum, bool includeMinimum,
        bool includeMaximum);

    /// <summary>
    ///     Includes the specified field and value range to the search query.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <param name="minimum">
    ///     The minimum value.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value.
    /// </param>
    /// <param name="includeMinimum">
    ///     Whether it should include minimum value.
    /// </param>
    /// <param name="includeMaximum">
    ///     Whether it should include maximum value.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder WithinRange(string name, TimeSpan? minimum, TimeSpan? maximum, bool includeMinimum,
        bool includeMaximum);

    /// <summary>
    ///     Marks the search as mandatory.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder Mandatory();

    /// <summary>
    ///     Marks the search as a forbidden.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder Forbidden();

    /// <summary>
    ///     Applied on string clauses, the searched value will not be tokenized.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder NoTokenize();

    /// <summary>
    ///     Applied on string clauses, it removes the default Prefix mechanism.
    ///     Like 'broadcast' won't return 'broadcasting'.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder ExactMatch();

    /// <summary>
    ///     Apply a specific weight.
    /// </summary>
    /// <param name="weight">
    ///     A value greater than zero, by which the score will be multiplied.
    ///     If greater than 1, it will improve the weight of a clause.
    /// </param>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder Weighted(float weight);

    /// <summary>
    ///     Defines a clause as a filter, so that it only affect the results
    ///     of the other clauses. For instance, if the other clauses returns
    ///     nothing, even if this filter has matches the end result will be
    ///     empty. It's like a two-pass query.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder AsFilter();

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortBy(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByBoolean(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByString(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByInteger(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByLong(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByFloat(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByDouble(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByDateTimeOffset(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByDateTime(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByDateOnly(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByTimeOnly(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByTimeSpan(string name);

    /// <summary>
    ///     Sort by field.
    /// </summary>
    /// <param name="name">
    ///     The field name.
    /// </param>
    /// <returns>
    /// 	The current <see cref="IQueryBuilder" /> so other actions
    /// 	can be chained.
    /// </returns>
    IQueryBuilder SortByEnum(string name);

    /// <summary>
    ///     Orders ascending.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder Ascending();

    /// <summary>
    ///     Slices the query results.
    /// </summary>
    /// <returns>
    ///     The current <see cref="IQueryBuilder" /> so other actions
    ///     can be chained.
    /// </returns>
    IQueryBuilder Slice(int limit);

    /// <summary>
    ///     Builds the search query definition.
    /// </summary>
    /// <returns>
    ///     A <see cref="QueryDefinition" /> object.
    /// </returns>
    QueryDefinition Build();
}