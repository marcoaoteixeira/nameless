namespace Nameless.EventSourcing.Events {

    /// <summary>
    /// Event Store
    /// </summary>
    public interface IEventStore {

        #region Methods

        /// <summary>
        /// Saves the events.
        /// </summary>
        /// <param name="events">The collection event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the method execution.</returns>
        Task SaveAsync(IEnumerable<IEvent> events, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all events for the aggregate ID, from specific version.
        /// </summary>
        /// <param name="aggregateID">The aggregate ID.</param>
        /// <param name="fromVersion">From what version will begin load.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An <see cref="IAsyncEnumerable{IEvent}"/> instance for enumeration.</returns>
        IAsyncEnumerable<IEvent> GetAsync(Guid aggregateID, int? fromVersion, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the last events for the aggregate.
        /// </summary>
        /// <param name="aggregateID">The aggregate ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{IEvent}"/> representing the method execution.</returns>
        Task<IEvent> GetLastEventAsync(Guid aggregateID, CancellationToken cancellationToken = default);

        #endregion Methods
    }
}
