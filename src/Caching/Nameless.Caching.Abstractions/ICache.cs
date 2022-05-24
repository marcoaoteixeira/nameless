namespace Nameless.Caching {

    public interface ICache {

        #region Methods

        Task SetAsync(string key, object value, CacheEntryOptions? options = null, CancellationToken cancellationToken = default);

        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

        #endregion
    }
}