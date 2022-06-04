namespace Nameless.Persistence {

    /// <summary>
    /// Exposes methods to define a data writer.
    /// </summary>
    public interface IWriter {

        #region Methods

        /// <summary>
        /// Saves all entities (or almost) in the <see cref="SaveInstructionCollection{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="instructions">The instruction collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of records affected by the instructions.
        /// </returns>
        Task<int> SaveAsync<TEntity>(SaveInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class;

        /// <summary>
        /// Deletes all entities (or almost) in the <see cref="DeleteInstructionCollection{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="instructions">The instruction collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The number of records affected by the instructions.
        /// </returns>
        Task<int> DeleteAsync<TEntity>(DeleteInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class;

        #endregion
    }
}