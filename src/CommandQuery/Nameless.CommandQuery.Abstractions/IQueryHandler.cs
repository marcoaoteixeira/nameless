namespace Nameless.CommandQuery {

    /// <summary>
    /// Query handler interface.
    /// </summary>
    /// <typeparam name="TQuery">Type of the query.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult> {

        #region Methods

        /// <summary>
        /// Handles the query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}" /> representing the query execution.</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);

        #endregion Methods
    }
}