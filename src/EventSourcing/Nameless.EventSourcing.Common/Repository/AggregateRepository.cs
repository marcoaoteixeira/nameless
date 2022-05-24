using Nameless.EventSourcing.Domains;
using Nameless.EventSourcing.Events;
using Nameless.EventSourcing.Snapshots;
using Nameless.Services;

namespace Nameless.EventSourcing.Repository {

    public class AggregateRepository : IAggregateRepository {

        #region Private Read-Only Fields

        private readonly IAggregateFactory _aggregateFactory;
        private readonly IClock _clock;
        private readonly IEventPublisher _eventPublisher;
        private readonly IEventStore _eventStore;
        private readonly ISnapshotStore _snapshotStore;
        private readonly ISnapshotStrategy _snapshotStrategy;

        #endregion

        #region Public Constructors

        public AggregateRepository(IAggregateFactory aggregateFactory, IClock clock, IEventPublisher eventPublisher, IEventStore eventStore, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy) {
            Ensure.NotNull(aggregateFactory, nameof(aggregateFactory));
            Ensure.NotNull(clock, nameof(clock));
            Ensure.NotNull(eventPublisher, nameof(eventPublisher));
            Ensure.NotNull(eventStore, nameof(eventStore));
            Ensure.NotNull(snapshotStore, nameof(snapshotStore));
            Ensure.NotNull(snapshotStrategy, nameof(snapshotStrategy));

            _aggregateFactory = aggregateFactory;
            _clock = clock;
            _eventPublisher = eventPublisher;
            _eventStore = eventStore;
            _snapshotStore = snapshotStore;
            _snapshotStrategy = snapshotStrategy;
        }

        #endregion

        #region Private Methods

        private async Task<TAggregate?> GetFromSnapshotStoreAsync<TAggregate>(Guid aggregateID, CancellationToken cancellationToken) where TAggregate : AggregateRoot {
            var isSnapshotable = _snapshotStrategy.IsSnapshottable(typeof(TAggregate));
            if (!isSnapshotable) { return default; }

            var snapshot = await _snapshotStore.GetAsync(aggregateID, cancellationToken);
            if (snapshot == null) { return default; }

            if (_aggregateFactory.Create<TAggregate>() is not Snapshottable aggregate) {
                return default;
            }

            aggregate.ApplySnapshot(snapshot);
            var eventEnumerator = _eventStore.GetAsync(aggregateID, fromVersion: snapshot.Version + 1, cancellationToken).GetAsyncEnumerator(cancellationToken);
            var events = new List<IEvent>();
            while (await eventEnumerator.MoveNextAsync()) {
                events.Add(eventEnumerator.Current);
            }
            aggregate.LoadFromHistory(events);

            return aggregate as TAggregate;
        }

        private async Task<TAggregate?> GetFromEventStoreAsync<TAggregate>(Guid aggregateID, CancellationToken cancellationToken) where TAggregate : AggregateRoot {
            var eventEnumerator = _eventStore.GetAsync(aggregateID, fromVersion: 0, cancellationToken).GetAsyncEnumerator(cancellationToken);
            var events = new List<IEvent>();
            while (await eventEnumerator.MoveNextAsync()) {
                events.Add(eventEnumerator.Current);
            }

            if (_aggregateFactory.Create<TAggregate>() is not TAggregate aggregate) { return default; }

            aggregate.LoadFromHistory(events);

            return aggregate;
        }

        #endregion

        #region IRepository<TAggregate> Members

        public Task<TAggregate?> GetAsync<TAggregate>(Guid aggregateID, CancellationToken cancellationToken = default) where TAggregate : AggregateRoot {
            return GetFromSnapshotStoreAsync<TAggregate>(aggregateID, cancellationToken) ?? GetFromEventStoreAsync<TAggregate>(aggregateID, cancellationToken);
        }

        public async Task SaveAsync(AggregateRoot? aggregate, CancellationToken cancellationToken = default) {
            if (aggregate == null) { throw new ArgumentNullException(nameof(aggregate)); }

            if (aggregate.HasUncommittedEvents()) {
                var expectedVersion = aggregate.LastCommittedVersion;
                var lastEvent = await _eventStore.GetLastEventAsync(aggregate.AggregateID, cancellationToken);
                if (lastEvent != null && expectedVersion == (int)StreamState.NoStream) {
                    throw new InvalidOperationException($"Aggregate ({lastEvent.AggregateID}) can't be created as it already exists with version {lastEvent.Version + 1}");
                }
                if (lastEvent != null && (lastEvent.Version + 1) != expectedVersion) {
                    throw new ConcurrencyException($"Aggregate {lastEvent.AggregateID} has been modified externally and has an updated state. Can't commit changes.");
                }

                var eventsToCommit = aggregate.GetUncommittedEvents();
                eventsToCommit.Each((evt, idx) => {
                    evt.TimeStamp = _clock.OffsetUtcNow;
                    evt.Version = aggregate.CurrentVersion + idx + 1;
                });

                await _eventStore.SaveAsync(eventsToCommit, cancellationToken);
                foreach (var evt in eventsToCommit) {
                    await _eventPublisher.PublishAsync(evt, cancellationToken);
                }

                var isSnapshottable = _snapshotStrategy.IsSnapshottable(aggregate.GetType());
                var shouldMakeSnapshot = _snapshotStrategy.ShouldMakeSnapshot(aggregate);
                if (isSnapshottable && shouldMakeSnapshot) {
                    var snapshottable = (Snapshottable)aggregate;
                    var snapshot = snapshottable.TakeSnapshot();
                    await _snapshotStore.SaveAsync(snapshot, cancellationToken);
                }

                aggregate.MarkAsCommitted();
            }
        }

        #endregion
    }
}
