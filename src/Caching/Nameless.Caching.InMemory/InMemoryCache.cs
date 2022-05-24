using System.Runtime.Caching;

namespace Nameless.Caching.InMemory {

    public sealed class InMemoryCache : ICache, IDisposable {

        #region Public Static Read-Only Fields

        /// <summary>
        /// Gets the cache name.
        /// </summary>
        public static readonly string DefaultCacheName = nameof(InMemoryCache);

        #endregion

        #region Private Fields

        private MemoryCache? _cache;
        private bool _disposed;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="InMemoryCache"/>.
        /// </summary>
        public InMemoryCache() : this(DefaultCacheName) { }

        /// <summary>
        /// Initializes a new instance of <see cref="InMemoryCache"/>.
        /// </summary>
        /// <param name="cacheName">The name of the cache</param>
        public InMemoryCache(string cacheName) {
            _cache = new MemoryCache(cacheName);
        }

        #endregion

        #region Destructor

        ~InMemoryCache() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Static Methods

        private static void SetEvictionCallback(CacheItemPolicy policy, CacheEntryOptions? options) {
            if (options is InMemoryCacheEntryOptions opts && opts.EvictionCallback != null) {
                policy.RemovedCallback += (args) => opts.EvictionCallback(args.CacheItem.Key, args.CacheItem.Value);
            }
        }

        private static void SetTimeExpiration(CacheItemPolicy policy, CacheEntryOptions? options) {
            if (options == null) { return; }

            if (options.AbsoluteExpiration.HasValue) {
                policy.AbsoluteExpiration = options.AbsoluteExpiration.Value;
            }

            if (options.SlidingExpiration.HasValue) {
                policy.SlidingExpiration = options.SlidingExpiration.Value;
            }
        }

        #endregion

        #region Private Methods

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                if (_cache != null) {
                    _cache.Dispose();
                }
            }

            _cache = null;
            _disposed = true;
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void OnChangeTokenCallback(object value) {
            BlockAccessAfterDispose();

            if (value is string key) {
                _cache!.Remove(key);
            }
        }

        private void SetChangeTokens(string? key, CacheEntryOptions? options) {
            if (options is InMemoryCacheEntryOptions opts) {
                foreach (var changeToken in opts.ChangeTokens) {
                    changeToken.RegisterChangeCallback(OnChangeTokenCallback, key);
                }
            }
        }

        #endregion

        #region ICache Members

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) {
            BlockAccessAfterDispose();

            cancellationToken.ThrowIfCancellationRequested();

            var value = _cache!.Get(key);
            var result = value != null ? (T)value : default;

            return Task.FromResult(result);
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default) {
            BlockAccessAfterDispose();

            cancellationToken.ThrowIfCancellationRequested();

            var result = _cache!.Remove(key) != null;

            return Task.FromResult(result);
        }

        public Task SetAsync(string key, object value, CacheEntryOptions? options, CancellationToken cancellationToken = default) {
            BlockAccessAfterDispose();

            cancellationToken.ThrowIfCancellationRequested();

            var policy = new CacheItemPolicy();

            SetEvictionCallback(policy, options);
            SetChangeTokens(key, options);
            SetTimeExpiration(policy, options);

            _cache!.Set(key, value, policy);

            return Task.CompletedTask;
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}