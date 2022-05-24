using Nameless.EventSourcing.Domains;

namespace Nameless.EventSourcing.Repository {

    /// <summary>
    /// Aggregate repository
    /// </summary>
    public interface IAggregateRepository {

		#region Public Methods

		/// <summary>
		/// Saves an aggregate.
		/// </summary>
		/// <param name="aggregate">The aggregate.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A <see cref="Task"/> representing the method execution.</returns>
		Task SaveAsync(AggregateRoot? aggregate, CancellationToken cancellationToken = default);

		/// <summary>
		/// Retrieves an aggregate.
		/// </summary>
		/// <param name="aggregateID">The aggregate ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A <see cref="Task{TAggregate}"/> representing the method execution.</returns>
		Task<TAggregate?> GetAsync<TAggregate>(Guid aggregateID, CancellationToken cancellationToken = default) where TAggregate : AggregateRoot;

		#endregion
	}
}
