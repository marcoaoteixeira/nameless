using Nameless.PubSub;

namespace Nameless.EventSourcing.Events {

    public class EventPublisher : IEventPublisher {

        #region Private Read-Only Fields

        private readonly IPublisher _publisher;
        private readonly IEventSerializer _serializer;

        #endregion

        #region Public Constructors

        public EventPublisher(IPublisher publisher, IEventSerializer serializer) {
            Prevent.Null(publisher, nameof(publisher));
            Prevent.Null(serializer, nameof(serializer));

            _publisher = publisher;
            _serializer = serializer;
        }

        #endregion

        #region IEventPublisher Members

        /// <inheritdoc />
        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default) where TEvent : IEvent {
            Prevent.Null(evt, nameof(evt));

            var message = new Message {
                Type = evt.GetType().FullName,
                Payload = _serializer.Serialize(evt)
            };

            return _publisher.PublishAsync(evt.GetType().FullName!, message, cancellationToken);
        }

        #endregion
    }
}
