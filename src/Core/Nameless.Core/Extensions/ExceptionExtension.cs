using System.Runtime.InteropServices;
using System.Security;

namespace Nameless {

    /// <summary>
    /// Extension methods for <see cref="Exception"/>.
    /// </summary>
    public static class ExceptionExtension {

        #region Public Static Methods

        /// <summary>
        /// Returns <c>true</c> if is a fatal exception.
        /// </summary>
        /// <param name="self">The self <see cref="Exception"/>.</param>
        /// <returns><c>true</c> if is fatal, otherwise, <c>false</c>.</returns>
        public static bool IsFatal(this Exception self) {
            return self is FatalException ||
                self is StackOverflowException ||
                self is OutOfMemoryException ||
                self is AccessViolationException ||
                self is AppDomainUnloadedException ||
                self is ThreadAbortException ||
                self is SecurityException ||
                self is SEHException;
        }

        #endregion Public Static Methods
    }
}