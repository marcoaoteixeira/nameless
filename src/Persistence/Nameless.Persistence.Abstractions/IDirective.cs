namespace Nameless.Persistence {

    /// <summary>
    /// Defines methods for directives.
    /// A directive can be a procedure, for example.
    /// /// </summary>
    /// <typeparam name="TResult">Type of the result</typeparam>
	public interface IDirective<TResult> {

        #region Methods

        /// <summary>
        /// Executes the directive.
        /// </summary>
        /// <param name="parameters">The directive parameters.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <returns>A dynamic representing the directive execution.</returns>
        Task<TResult?> ExecuteAsync(ParameterSet parameters, CancellationToken cancellationToken = default);

        #endregion Methods
    }
}