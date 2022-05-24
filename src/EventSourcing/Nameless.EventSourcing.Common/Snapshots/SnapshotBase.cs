namespace Nameless.EventSourcing.Snapshots {

    public abstract class SnapshotBase : ISnapshot {

        #region ISnapshot Members

        public Guid AggregateID { get; }

        public int Version { get; }

        #endregion

        #region Protected Constructors

        protected SnapshotBase(Guid aggregateID, int version) {
            AggregateID = aggregateID;
            Version = version;
        }

        #endregion
    }
}
