namespace Nameless.Notification {

    [NullObject]
    public sealed class NullNotifier : INotifier {

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of NullNotifier.
        /// </summary>
        public static INotifier Instance { get; } = new NullNotifier();

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static NullNotifier() { }

        #endregion

        #region Private Constructors

        // Prevents the class from being constructed.
        private NullNotifier() { }

        #endregion

        #region INotifier Members

        /// <inheritdoc />
        public void Add(NotifyType type, string message) { }

        /// <inheritdoc />
        public IEnumerable<NotifyEntry> Flush() => Array.Empty<NotifyEntry>();

        #endregion
    }
}
