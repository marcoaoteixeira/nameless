using Nameless.Serialization;
using RabbitMQ.Client;

namespace Nameless.PubSub.RabbitMQ {

    internal sealed class InnerPublisher : IPublisher {

        #region Private Read-Only Fields

        private readonly Exchange _exchange;
        private readonly ISerializer _serializer;

        #endregion

        #region Private Fields

        private IConnection? _connection;
        private IModel? _channel;
        private bool _disposed;

        #endregion

        #region Internal Constructors

        internal InnerPublisher(IConnectionFactory factory, Exchange exchange, ISerializer serializer) {
            Prevent.Null(factory, nameof(factory));
            Prevent.Null(exchange, nameof(exchange));
            Prevent.Null(serializer, nameof(serializer));

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _exchange = exchange;
            _serializer = serializer;

            Initialize();
        }

        #endregion

        #region Destructor

        ~InnerPublisher() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void Initialize() {
            _channel!.ExchangeDeclare(
                exchange: _exchange.Name,
                type: _exchange.Type.GetDescription(),
                durable: _exchange.Durable,
                autoDelete: _exchange.AutoDelete,
                arguments: null
            );
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _channel?.Dispose();
                _connection?.Dispose();
            }

            _channel = null;
            _connection = null;
            _disposed = true;
        }

        #endregion

        #region IDisposable Members

        void IDisposable.Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(obj: this);
        }

        #endregion

        #region IPublisher Members

        Task IPublisher.PublishAsync(string topic, Message message, CancellationToken token) {
            BlockAccessAfterDispose();

            var body = _serializer.Serialize(message);

            token.ThrowIfCancellationRequested();

            _channel.BasicPublish(
                exchange: topic,
                routingKey: string.Empty,
                basicProperties: null,
                body: body
            );

            return Task.CompletedTask;
        }

        #endregion
    }
}