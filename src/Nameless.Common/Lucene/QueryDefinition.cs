using Lucene.Net.Search;

namespace Nameless.Lucene;

/// <summary>
///     Represents a definition for a search query, including the query itself,
///     sorting options, and pagination settings.
/// </summary>
public record QueryDefinition {
    /// <summary>
    ///     Gets the Lucene query to be executed.
    /// </summary>
    public Query Query { get; }

    /// <summary>
    ///     Gets the sorting options for the query results.
    ///     Defaults to relevance sorting if not specified.
    /// </summary>
    public Sort Sort { get; }

    /// <summary>
    ///     Gets the maximum number of results to return from the query.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    ///     Initializes a new instance of the
    ///     <see cref="QueryDefinition" /> class.
    /// </summary>
    /// <param name="query">
    ///     The Lucene query to be executed.
    /// </param>
    /// <param name="sort">
    ///     The sorting options for the query results.
    /// </param>
    /// <param name="limit">
    ///     The maximum number of results to return from the query.
    ///     Defaults to <c>10</c> if not specified.
    /// </param>
    public QueryDefinition(Query query, Sort? sort = null, int limit = 10) {
        Query = query;
        Sort = sort ?? Sort.RELEVANCE;
        Limit = Throws.When.OutOfRange(
            limit,
            minimumValue: 1,
            maximumValue: LuceneConstants.MaximumQueryResults
        );
    }
}