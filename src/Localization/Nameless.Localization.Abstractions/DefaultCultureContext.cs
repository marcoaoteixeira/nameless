using System.Globalization;

namespace Nameless.Localization {


    /// <summary>
    /// Singleton Pattern implementation for <see cref="DefaultCultureContext" />. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    [Singleton]
    public sealed class DefaultCultureContext : ICultureContext {

        #region Private Static Read-Only Fields

        private static readonly ICultureContext _instance = new DefaultCultureContext();

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of <see cref="DefaultCultureContext" />.
        /// </summary>
        public static ICultureContext Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static DefaultCultureContext() { }

        #endregion

        #region Private Constructors

        private DefaultCultureContext() { }

        #endregion

        #region ICultureContext Members

        public CultureInfo GetCulture() => Thread.CurrentThread.CurrentCulture;

        #endregion
    }

}
