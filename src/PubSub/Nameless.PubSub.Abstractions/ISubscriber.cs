namespace Nameless.PubSub {

    /// <summary>
    /// Defines methods to implement a topic based subscriber.
    /// </summary>
    public interface ISubscriber : IDisposable {

        #region Methods

        /// <summary>
        /// Subscribes a handler for notification.
        /// </summary>
        /// <param name="topic">The subscription topic.</param>
        /// <param name="handler">The handler.</param>
        /// <returns>An instance of <see cref="Subscription" />.</returns>
        Subscription Subscribe(string topic, Action<Message> handler);

        /// <summary>
        /// Unsubscribes a handler from notification.
        /// </summary>
        /// <param name="subscription">The subscription.</param>
        /// <returns><c>true</c> if can unsubscribe; otherwise, <c>false</c>.</returns>
        bool Unsubscribe(Subscription subscription);

        #endregion
    }
}