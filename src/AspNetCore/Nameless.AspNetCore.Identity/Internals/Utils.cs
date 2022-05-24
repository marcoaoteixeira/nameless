using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity {

    internal static class Utils {

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

        internal static T Parse<T>(object value) {
            var result = Parse(value, typeof(T));
            if (result == null) { return default!; }
            return (T)result;
        }

        internal static object? Parse(object value, Type convertType) {
            if (value == null) { return null; }

            if (typeof(IConvertible).IsAssignableFrom(value.GetType())) {
                return Convert.ChangeType(value, convertType);
            }

            if (Guid.TryParse(value.ToString(), out Guid result)) {
                return result;
            }

            return null;
        }

        #endregion
    }
}
