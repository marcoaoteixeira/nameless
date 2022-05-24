namespace Nameless.Notification {

    /// <summary>
    /// Represents a UI notification entry.
    /// </summary>
    public sealed class NotifyEntry {

        #region Public Properties

        /// <summary>
        /// Gets or sets the notification type.
        /// </summary>
        public NotifyType Type { get; set; }

        /// <summary>
        /// Gets or sets the notification message.
        /// </summary>
        public string? Message { get; set; }

        #endregion
    }
}
