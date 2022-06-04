using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity {

    internal static class Internals {

        #region Internal Static Methods

        internal static IdentityResult IdentityResultContinuation(Task continuation) {
            if (continuation.Exception is AggregateException ex) {
                return IdentityResult.Failed(new IdentityError {
                    Description = ex.Flatten().Message
                });
            }

            if (continuation.IsCanceled) {
                return IdentityResult.Failed(new IdentityError {
                    Description = "Task cancelled by the user."
                });
            }

            return IdentityResult.Success;
        }

        #endregion
    }
}
