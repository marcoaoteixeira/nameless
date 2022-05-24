namespace Nameless.Localization {

    /// <summary>
    /// Singleton Pattern implementation for Localizer. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    [NullObject]
    public sealed class NullStringLocalizer : IStringLocalizer {

        #region Private Static Read-Only Fields

        private static readonly IStringLocalizer _instance = new NullStringLocalizer();

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of Localizer.
        /// </summary>
        public static IStringLocalizer Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static NullStringLocalizer() { }

        #endregion

        #region Private Constructors

        private NullStringLocalizer() { }

        #endregion

        #region ILocalizer Members

        public LocaleString this[string text, int count = -1, params object[] args] => new(Thread.CurrentThread.CurrentUICulture, text, text, args);

        public IEnumerable<LocaleString> List(bool includeParentCultures) => Enumerable.Empty<LocaleString>();

        #endregion
    }
}