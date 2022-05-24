using Nameless.EventSourcing.Domains;

namespace Nameless.EventSourcing.Repository {

    /// <summary>
    /// Contract to an aggregate session.
    /// </summary>
    public interface IAggregateSession {

        #region Public Methods

        /// <summary>
        /// Attaches an aggregate to the current session.
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// <param name="aggregate">The instance of the aggregate.</param>
        void Attach<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot;

        /// <summary>
        /// Detaches an aggregate from the current session.
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// <param name="aggregate">The instance of the aggregate.</param>
        void Detach<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot;

        /// <summary>
        /// Detaches all aggregates from the current session.
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// </summary>
        void DetachAll<TAggregate>() where TAggregate : AggregateRoot;

        /// <summary>
        /// Retrieves an aggregate from the current session.
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// <param name="aggregateID">The aggregate ID.</param>
        /// <returns>The aggregate in session.</returns>
        TAggregate? Get<TAggregate>(Guid aggregateID) where TAggregate : AggregateRoot;

        /// <summary>
        /// Commits all changes.
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// <param name="cancellationToken">The cancellation token.</param>
        Task CommitAsync<TAggregate>(CancellationToken cancellationToken = default) where TAggregate : AggregateRoot;

        #endregion
    }
}
