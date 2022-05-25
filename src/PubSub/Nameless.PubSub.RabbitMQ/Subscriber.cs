using System.Diagnostics.CodeAnalysis;
using Nameless.Serialization;
using RabbitMQ.Client;

namespace Nameless.PubSub.RabbitMQ {

    public sealed class Subscriber : ISubscriber {

        #region Private Read-Only Fields

        private readonly IConnectionFactory _factory;
        private readonly ISerializer _serializer;
        private readonly PubSubOptions _options;
        private readonly Dictionary<string, ISubscriber> _cache = new();
        private readonly object _syncLock = new();

        #endregion

        #region Private Fields

        private bool _disposed;

        #endregion

        #region Public Constructors

        public Subscriber(IConnectionFactory factory, ISerializer serializer, PubSubOptions? options = null) {
            Prevent.Null(factory, nameof(factory));
            Prevent.Null(serializer, nameof(serializer));

            _factory = factory;
            _serializer = serializer;
            _options = options ?? new();
        }

        #endregion

        #region Destructor

        ~Subscriber() => Dispose(disposing: false);

        #endregion

        #region Private Methods

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
        }


        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                lock (_syncLock) {
                    _cache.Values.Each(item => item.Dispose());
                }
            }

            _disposed = true;
        }

        #endregion

        #region ISubscriber Members

        public Subscription Subscribe(string topic, Action<Message> handler) {
            BlockAccessAfterDispose();

            if (topic == null || !topic.Trim().Any()) { throw new ArgumentException("Parameter cannot be null, empty or white spaces.", nameof(topic)); }
            if (handler == null) { throw new ArgumentNullException(nameof(handler)); }

            var exchange = _options.Exchanges.FirstOrDefault(_ => _.Name == topic);
            if (exchange == null || exchange.Name == null) {
                throw new InvalidOperationException("Exchange not configured.");
            }

            lock (_syncLock) {
                if (!_cache.ContainsKey(exchange.Name)) {
                    _cache[exchange.Name] = new InnerSubscriber(_factory, exchange, _serializer);
                }
                return _cache[exchange.Name].Subscribe(exchange.Name, handler);
            }
        }

        public bool Unsubscribe(Subscription subscription) {
            BlockAccessAfterDispose();

            Prevent.Null(subscription, nameof(subscription));
            lock (_syncLock) {
                var topic = subscription.Topic;
                return _cache.ContainsKey(topic) &&
                       _cache[topic].Unsubscribe(subscription);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}