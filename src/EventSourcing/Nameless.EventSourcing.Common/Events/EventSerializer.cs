using Nameless.Serialization;

namespace Nameless.EventSourcing.Events {

    public class EventSerializer : IEventSerializer {

        #region Private Read-Only Fields

        private readonly ISerializer _serializer;

        #endregion

        #region Public Constructors

        public EventSerializer(ISerializer serializer) {
            Prevent.Null(serializer, nameof(serializer));

            _serializer = serializer;
        }

        #endregion

        #region IEventSerializer Members

        public byte[]? Serialize(IEvent evt) {
            Prevent.Null(evt, nameof(evt));

            return _serializer.Serialize(evt);
        }

        public IEvent? Deserialize(Type eventType, byte[] payload) {
            Prevent.Null(eventType, nameof(eventType));
            Prevent.Null(payload, nameof(payload));

            if (!typeof(IEvent).IsAssignableFrom(eventType)) {
                throw new InvalidCastException($"Event type is not assignable to {typeof(IEvent).FullName}");
            }

            return _serializer.Deserialize(eventType, payload) as IEvent;
        }

        #endregion
    }
}
