namespace Nameless.EventSourcing.Snapshots {

    public interface ISnapshotStore {

        #region Methods

        /// <summary>
        /// Saves a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the method execution.</returns>
        Task SaveAsync(ISnapshot snapshot, CancellationToken cancellationToken = default);
        /// <summary>
        /// Retrieves a snapshot.
        /// </summary>
        /// <param name="aggregateID">The aggregate ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{ISnapshot}"/> representing the method execution.</returns>
        Task<ISnapshot> GetAsync(Guid aggregateID, CancellationToken cancellationToken = default);

        #endregion
    }
}
