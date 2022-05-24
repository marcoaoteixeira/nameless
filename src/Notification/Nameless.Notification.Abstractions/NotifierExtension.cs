namespace Nameless.Notification {

    /// <summary>
    /// Extension methods for <see cref="INotifier"/>.
    /// </summary>
    public static class NotifierExtension {

        #region Public Static Methods

        /// <summary>
        /// Adds a new UI notification of type Information
        /// </summary>
        /// <seealso cref="INotifier.Add(NotifyType, string)"/>
        /// <param name="source">The instance of <see cref="INotifier"/>.</param>
        /// <param name="message">A message to display</param>
        public static void Information(this INotifier source, string message) {
            if (source == null) { return; }

            source.Add(NotifyType.Information, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Warning
        /// </summary>
        /// <seealso cref="INotifier.Add(NotifyType, string)"/>
        /// <param name="source">The instance of <see cref="INotifier"/>.</param>
        /// <param name="message">A message to display</param>
        public static void Warning(this INotifier source, string message) {
            if (source == null) { return; }

            source.Add(NotifyType.Warning, message);
        }

        /// <summary>
        /// Adds a new UI notification of type Error
        /// </summary>
        /// <seealso cref="INotifier.Add(NotifyType, string)"/>
        /// <param name="source">The instance of <see cref="INotifier"/>.</param>
        /// <param name="message">A message to display</param>
        public static void Error(this INotifier source, string message) {
            if (source == null) { return; }

            source.Add(NotifyType.Error, message);
        }

        #endregion
    }
}
