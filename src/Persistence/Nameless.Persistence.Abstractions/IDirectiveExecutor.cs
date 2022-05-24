namespace Nameless.Persistence {

    public interface IDirectiveExecutor {

        #region Methods

        /// <summary>
        /// Executes a directive asynchronous.
        /// </summary>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <typeparam name="TDirective">The directive type.</typeparam>
        /// <param name="parameters">The directive parameters.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <returns>The <see cref="Task{TResult}"/> representing the directive execution.</returns>
        Task<TResult?> ExecuteDirectiveAsync<TResult, TDirective>(ParameterSet parameters, CancellationToken cancellationToken = default)
            where TDirective : IDirective<TResult?>;

        #endregion Methods
    }
}