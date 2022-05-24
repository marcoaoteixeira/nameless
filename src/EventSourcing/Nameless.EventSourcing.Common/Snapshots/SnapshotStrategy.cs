using Nameless.EventSourcing.Domains;

namespace Nameless.EventSourcing.Snapshots {

    public class SnapshotStrategy : ISnapshotStrategy {

        #region Private Read-Only Fields

        private readonly EventSourceOptions _opts;

        #endregion

        #region Public Constructors

        public SnapshotStrategy(EventSourceOptions? opts = null) {
            _opts = opts ?? new();
        }

        #endregion

        #region ISnapshotStrategy Members

        public bool IsSnapshottable(Type? aggregateType) {
            if (aggregateType == null) { return false; }

            if (typeof(Snapshottable).IsAssignableFrom(aggregateType)) { return true; }

            return IsSnapshottable(aggregateType.BaseType);
        }

        public bool ShouldMakeSnapshot(AggregateRoot aggregate) {
            if (!_opts.TakeSnapshots) { return false; }

            var totalEventsToCommit = aggregate.GetUncommittedEvents().Count();

            var currentVersionMustBeGreaterOrEqualToSnapshotFrequency = aggregate.CurrentVersion >= _opts.SnapshotFrequency;
            var totalEventsToCommitMustBeGreaterOrEqualToSnapshotFrequency = totalEventsToCommit >= _opts.SnapshotFrequency;
            var modOfCurrentVersionAndSnapshotFrequencyMustBeLowerThenTotalEventsToCommit = (aggregate.CurrentVersion % _opts.SnapshotFrequency) < totalEventsToCommit;
            var modOfCurrentVersionAndSnapshotFrequencyMustBeZero = (aggregate.CurrentVersion % _opts.SnapshotFrequency) == 0;

            return currentVersionMustBeGreaterOrEqualToSnapshotFrequency && (
                totalEventsToCommitMustBeGreaterOrEqualToSnapshotFrequency ||
                modOfCurrentVersionAndSnapshotFrequencyMustBeLowerThenTotalEventsToCommit ||
                modOfCurrentVersionAndSnapshotFrequencyMustBeZero
            );
        }

        #endregion
    }
}
