namespace Nameless.Services {

    /// <summary>
    /// Singleton Pattern implementation for <see cref="SystemClock" />. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    [Singleton]
    public sealed class SystemClock : IClock {

        #region Private Static Read-Only Fields

        private static readonly IClock _instance = new SystemClock();

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of <see cref="SystemClock" />.
        /// </summary>
        public static IClock Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static SystemClock() { }

        #endregion

        #region Private Constructors

        private SystemClock() { }

        #endregion

        #region IClock Members

        /// <inheritdoc/>
        public DateTime UtcNow => DateTime.UtcNow;
        /// <inheritdoc/>
        public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;

        #endregion
    }
}
