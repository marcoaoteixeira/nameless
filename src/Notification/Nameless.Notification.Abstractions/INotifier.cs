namespace Nameless.Notification {

    /// <summary>
    /// Defines the functionality for show notifications on the UI.
    /// </summary>
    public interface INotifier {

        #region Methods

        /// <summary>
        /// Adds a new UI notification
        /// </summary>
        /// <param name="type">
        /// The type of the notification (notifications with different types can be displayed differently)</param>
        /// <param name="message">A localized message to display</param>
        void Add(NotifyType type, string message);

        /// <summary>
        /// Flush all notifications added
        /// </summary>
        IEnumerable<NotifyEntry> Flush();

        #endregion Methods
    }
}
