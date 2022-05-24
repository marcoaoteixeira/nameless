namespace Nameless.EventSourcing.Snapshots {

    public interface ISnapshot {

        #region Properties

        Guid AggregateID { get; }
        int Version { get; }

        #endregion
    }
}
