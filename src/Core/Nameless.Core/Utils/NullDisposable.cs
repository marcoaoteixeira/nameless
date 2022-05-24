using System;

namespace Nameless {

    /// <summary>
    /// Singleton Pattern implementation for <see cref="NullDisposable" />. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    [NullObject]
    public sealed class NullDisposable : IDisposable {

        #region Private Static Read-Only Fields

        private static readonly IDisposable _instance = new NullDisposable();

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of <see cref="NullDisposable" />.
        /// </summary>
        public static IDisposable Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static NullDisposable() { }

        #endregion

        #region Private Constructors

        private NullDisposable() { }

        #endregion

        #region IDisposable Members

        public void Dispose() { }

        #endregion
    }
}