using Nameless.EventSourcing.Domains;

namespace Nameless.EventSourcing.Snapshots {

    public abstract class Snapshottable : AggregateRoot {

        #region Methods

        /// <summary>
        /// Creates a snapshot.
        /// </summary>
        /// <returns>A instance of the specific snapshot.</returns>
        public abstract ISnapshot TakeSnapshot();
        /// <summary>
        /// Creates a snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to apply.</param>
        public abstract void ApplySnapshot(ISnapshot snapshot);

        #endregion
    }
}
