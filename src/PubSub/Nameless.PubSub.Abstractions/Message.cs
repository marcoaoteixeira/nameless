namespace Nameless.PubSub {

    /// <summary>
    /// Defines a message for the publisher/subscriber
    /// </summary>
    public sealed class Message {

        #region Public Properties

        /// <summary>
        /// Gets or sets the type of the message
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the message payload.
        /// </summary>
        /// <value></value>
        public object? Payload { get; set; }

        #endregion
    }
}