using Nameless.Caching;

namespace Nameless.WebApplication.Web.Services {

    public sealed class CacheService : ICacheService {

        #region Private Read-Only Fields

        private readonly ICache _cache;

        #endregion

        #region Public Constructors

        public CacheService(ICache cache) {
            Prevent.Null(cache, nameof(cache));

            _cache = cache;
        }

        #endregion

        #region ICacheService Members

        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default) {
            Prevent.NullEmptyOrWhiteSpace(key, nameof(key));

            return _cache.RemoveAsync(key, cancellationToken);
        }

        public Task StoreAsync(string key, object value, DateTimeOffset expiration, CancellationToken cancellationToken = default) {
            Prevent.NullEmptyOrWhiteSpace(key, nameof(key));
            Prevent.Null(value, nameof(value));

            var opts = new CacheEntryOptions {
                AbsoluteExpiration = expiration
            };
            return _cache.SetAsync(key, value, opts, cancellationToken);
        }

        #endregion
    }
}
