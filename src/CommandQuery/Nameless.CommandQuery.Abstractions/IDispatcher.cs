namespace Nameless.CommandQuery {

    public interface IDispatcher {

        #region Methods

        /// <summary>
        /// Executes a command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the dispatch execution.</returns>
        Task ExecuteAsync(ICommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query.
        /// </summary>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}" /> representing the query execution.</returns>
        Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);

        #endregion Methods
    }
}