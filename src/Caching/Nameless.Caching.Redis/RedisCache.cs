using Nameless.Caching;
using Nameless.Serialization;
using StackExchange.Redis;

namespace Nameless.Caching.Redis {

    public class RedisCache : ICache {

        #region Private Read-Only Fields

        private readonly IDatabaseAsync _database;
        private readonly ISerializer _serializer;

        #endregion

        #region Public Constructors

        public RedisCache(IConnectionMultiplexer connection, ISerializer serializer) {
            Prevent.Null(connection, nameof(connection));
            Prevent.Null(serializer, nameof(serializer));

            _database = connection.GetDatabase();
            _serializer = serializer;
        }

        #endregion

        #region Private Methods

        private void SetExpirationDate(string key, CacheEntryOptions options) {
            if (options == null) { return; }

            // TODO: Set expiration keys absolute and sliding.
        }

        #endregion

        #region ICache Members

        public Task SetAsync(string? key, object? value, CacheEntryOptions? options = null, CancellationToken token = default) {
            using var memoryStream = new MemoryStream();
            _serializer.Serialize(memoryStream, value);
            var redisValue = RedisValue.CreateFrom(memoryStream);

            return _database.StringSetAsync(key, redisValue);
        }

        public Task<T?> GetAsync<T>(string? key, CancellationToken token = default) {
            return _database
                .StringGetAsync(key)
                .ContinueWith(antecedent => {
                    if (antecedent.CanContinue() && antecedent.Result.HasValue) {
                        var value = antecedent.Result.ToString();
                        return _serializer.Deserialize<T>(value.ToStream());
                    }
                    return default;
                });
        }

        public Task<bool> RemoveAsync(string? key, CancellationToken token = default) {
            if (key == null) { throw new ArgumentNullException(nameof(key)); }

            return _database.KeyDeleteAsync(key);
        }

        #endregion
    }
}