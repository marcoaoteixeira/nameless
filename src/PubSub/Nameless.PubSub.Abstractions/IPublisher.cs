namespace Nameless.PubSub {

    /// <summary>
    /// Defines methods to implement a topic based publisher.
    /// </summary>
    public interface IPublisher : IDisposable {

        #region Methods

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="message">The message.</param>
        /// <param name="token">The cancellation token.</param>
        Task PublishAsync(string topic, Message message, CancellationToken token = default);

        #endregion Methods
    }
}