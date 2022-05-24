namespace Nameless.EventSourcing.Events {

    public abstract class EventBase : IEvent {

        #region IEvent Members

        /// <inheritdoc />
        public Guid EventID { get; set; }
        /// <inheritdoc />
        public Guid AggregateID { get; set; }
        /// <inheritdoc />
        public int Version { get; set; }
        /// <inheritdoc />
        public DateTimeOffset TimeStamp { get; set; }

        #endregion

        #region Protected Constructors

        protected EventBase(Guid aggregateID) {
            EventID = Guid.NewGuid();
            AggregateID = aggregateID;
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override string ToString() => GetType().Name;

        #endregion
    }
}
