using Nameless.Helpers;

namespace Nameless.Caching {

    // I know, this is not the best solution...
    public static class CacheExtension {

        #region Public Static Methods

        public static void Set(this ICache self, string key, object value, CacheEntryOptions? options = null) {
            if (self == null) { return; }

            AsyncHelper.RunSync(() => self.SetAsync(key, value, options));
        }

        public static T? Get<T>(this ICache self, string key) {
            if (self == null) { return default; }

            return AsyncHelper.RunSync(() => self.GetAsync<T>(key));
        }

        public static bool Remove(this ICache self, string key) {
            if (self == null) { return false; }

            return AsyncHelper.RunSync(() => self.RemoveAsync(key));
        }

        #endregion
    }
}