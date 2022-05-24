using Nameless.Serialization;

namespace Nameless.EventSourcing.Events {

    public class EventSerializer : IEventSerializer {

        #region Private Read-Only Fields

        private readonly ISerializer _serializer;

        #endregion

        #region Public Constructors

        public EventSerializer(ISerializer serializer) {
            Ensure.NotNull(serializer, nameof(serializer));

            _serializer = serializer;
        }

        #endregion

        #region IEventSerializer Members

        public byte[]? Serialize(IEvent evt) {
            Ensure.NotNull(evt, nameof(evt));

            return _serializer.Serialize(evt);
        }

        public IEvent? Deserialize(Type eventType, byte[] payload) {
            Ensure.NotNull(eventType, nameof(eventType));
            Ensure.NotNull(payload, nameof(payload));

            if (!typeof(IEvent).IsAssignableFrom(eventType)) {
                throw new InvalidCastException($"Event type is not assignable to {typeof(IEvent).FullName}");
            }

            return _serializer.Deserialize(eventType, payload) as IEvent;
        }

        #endregion
    }
}
