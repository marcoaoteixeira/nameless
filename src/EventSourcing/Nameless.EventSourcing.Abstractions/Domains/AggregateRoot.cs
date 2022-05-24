using Nameless.EventSourcing.Events;

namespace Nameless.EventSourcing.Domains {

    /// <summary>
    /// Abstract aggregate root.
    /// </summary>
    public abstract class AggregateRoot {

        #region Private Property

        private IList<IEvent> UncommittedEvents { get; } = new List<IEvent>();
        private dynamic Proxy { get; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the aggregate ID.
        /// </summary>
        public Guid AggregateID { get; protected set; }

        /// <summary>
        /// Gets the aggregate current version.
        /// </summary>
        public int CurrentVersion { get; private set; } = (int)StreamState.NoStream;

        /// <summary>
        /// Gets the aggregate last committed version.
        /// </summary>
        public int LastCommittedVersion { get; private set; } = (int)StreamState.NoStream;

        #endregion

        #region Protected Constructors

        protected AggregateRoot() {
            Proxy = new PrivateReflectionDynamicObject(realObject: this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if there are uncommitted events.
        /// </summary>
        /// <returns>Returns <c>true</c> whether there are uncommitted events; otherwise <c>false</c>.</returns>
        public bool HasUncommittedEvents() {
            lock (UncommittedEvents) {
                return UncommittedEvents.Any();
            }
        }

        /// <summary>
        /// Retrieves all uncommitted changes
        /// </summary>
        /// <returns>A collection of uncommitted events.</returns>
        public IEnumerable<IEvent> GetUncommittedEvents() {
            lock (UncommittedEvents) {
                return UncommittedEvents.ToArray();
            }
        }

        public void MarkAsCommitted() {
            lock (UncommittedEvents) {
                LastCommittedVersion = CurrentVersion;
                UncommittedEvents.Clear();
            }
        }

        public void LoadFromHistory(IEnumerable<IEvent> history) {
            lock (UncommittedEvents) {
                ValidateHistory(history);
                foreach (var evt in history) {
                    if (evt.Version != (CurrentVersion + 1)) {
                        throw new EventsOutOfOrderException(evt.EventID);
                    }
                    Proxy.OnEvent(evt);
                    AggregateID = evt.AggregateID;
                    CurrentVersion++;
                }
                LastCommittedVersion = CurrentVersion;
            }
        }

        public bool Equals(AggregateRoot? obj) => obj != null && obj.AggregateID == AggregateID;

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override bool Equals(object? obj) => Equals(obj as AggregateRoot);

        /// <inheritdoc />
        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += AggregateID.GetHashCode() * 7;
            }
            return hash;
        }

        #endregion

        #region Protected Methods

        protected void ApplyEvent(IEvent evt) {
            lock (UncommittedEvents) {
                Proxy.OnEvent(evt);
                UncommittedEvents.Add(evt);
                CurrentVersion++;
            }
        }

        #endregion

        #region Private Static Methods

        private static void ValidateHistory(IEnumerable<IEvent> history) {
            if (history == null || !history.Any()) { return; }

            // let's assume that the first event is correct.
            var first = history.First();
            var aggregateID = first.AggregateID;

            // There is any event with a different aggregate ID ?
            var differentAggregateID = history.Any(_ => _.AggregateID != aggregateID);
            if (differentAggregateID) {
                throw new InvalidOperationException("There are one or more events with different aggregate association.");
            }

            // Checks if there is any event without ID
            var eventWithoutID = history.Any(_ => _.EventID == Guid.Empty);
            if (eventWithoutID) {
                throw new InvalidOperationException("There are one or more events without ID.");
            }
        }

        #endregion
    }
}
