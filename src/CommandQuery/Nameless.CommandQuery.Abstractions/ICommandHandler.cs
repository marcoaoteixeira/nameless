namespace Nameless.CommandQuery {

    /// <summary>
    /// Command handler interface.
    /// </summary>
    /// <typeparam name="TCommand">Type of the command.</typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand {

        #region Methods

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the command execution.</returns>
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);

        #endregion
    }
}