using Nameless.EventSourcing.Domains;

namespace Nameless.EventSourcing.Snapshots {

    public interface ISnapshotStrategy {

        #region Methods

        /// <summary>
        /// Verifies whether should make a snapshot of the aggregate root.
        /// </summary>
        /// <param name="aggregate">The aggregate root.</param>
        /// <returns><c>true</c> if should create a snapshot; otherwise <c>false</c>.</returns>
        bool ShouldMakeSnapshot(AggregateRoot aggregate);
        /// <summary>
        /// Verifies whether the aggregate type supports snapshot events.
        /// </summary>
        /// <param name="aggregateType">The aggregate type</param>
        /// <returns><c>true</c> if aggregate type can become a snapshot; otherwise <c>false</c>.</returns>
        bool IsSnapshottable(Type? aggregateType);

        #endregion
    }
}
