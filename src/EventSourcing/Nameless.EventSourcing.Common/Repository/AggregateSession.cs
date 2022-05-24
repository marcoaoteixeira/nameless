using Nameless.EventSourcing.Domains;

namespace Nameless.EventSourcing.Repository {

    public sealed class AggregateSession : IAggregateSession {

        #region Private Read-Only Fields

        private readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);
        private readonly IDictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly IAggregateRepository _aggregateRepository;

        #endregion

        #region Public Constructors

        public AggregateSession(IAggregateRepository repository) {
            Ensure.NotNull(repository, nameof(repository));

            _aggregateRepository = repository;
        }

        #endregion

        #region Private Static Methods

        private static string GetCacheKeyPrefix<TAggregate>() where TAggregate : AggregateRoot {
            return $"[{typeof(TAggregate).FullName}]";
        }

        private static string GetCacheKeyFor<TAggregate>(Guid aggregateID) where TAggregate : AggregateRoot {
            var prefix = GetCacheKeyPrefix<TAggregate>();
            return $"{prefix} : {aggregateID}";
        }

        #endregion

        #region Private Methods

        private IEnumerable<string> GetCurrentCacheKeys<TAggregate>() where TAggregate : AggregateRoot {
            var prefix = GetCacheKeyPrefix<TAggregate>();
            return _cache.Keys.Where(_ => _.StartsWith(prefix)).ToArray();
        }

        #endregion

        #region ISession Members

        public void Attach<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot {
            Ensure.NotNull(aggregate, nameof(aggregate));

            _semaphore.Wait();
            try {
                var key = GetCacheKeyFor<TAggregate>(aggregate.AggregateID);
                if (!_cache.ContainsKey(key)) {
                    _cache.Add(key, aggregate);
                }
            } finally { _semaphore.Release(); }
        }

        public void Detach<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot {
            Ensure.NotNull(aggregate, nameof(aggregate));

            _semaphore.Wait();
            try {
                var key = GetCacheKeyFor<TAggregate>(aggregate.AggregateID);
                if (_cache.ContainsKey(key)) {
                    _cache.Remove(key);
                }
            } finally { _semaphore.Release(); }
        }

        public void DetachAll<TAggregate>() where TAggregate : AggregateRoot {
            _semaphore.Wait();
            try {
                var keys = GetCurrentCacheKeys<TAggregate>();
                foreach (var key in keys) {
                    _cache.Remove(key);
                }
            } finally { _semaphore.Release(); }
        }

        public TAggregate? Get<TAggregate>(Guid aggregateID) where TAggregate : AggregateRoot {
            _semaphore.Wait();
            TAggregate? result = default;
            try {
                var key = GetCacheKeyFor<TAggregate>(aggregateID);
                if (_cache.TryGetValue(key, out object? aggregate)) {
                    result = aggregate as TAggregate;
                }
            } finally { _semaphore.Release(); }
            return result;
        }

        public async Task CommitAsync<TAggregate>(CancellationToken cancellationToken = default) where TAggregate : AggregateRoot {
            _semaphore.Wait(cancellationToken);
            try {
                var keys = GetCurrentCacheKeys<TAggregate>();
                foreach (var key in keys) {
                    if (_cache.TryGetValue(key, out object? aggregate)) {
                        cancellationToken.ThrowIfCancellationRequested();
                        await _aggregateRepository.SaveAsync(aggregate as TAggregate, cancellationToken);
                    }
                }
            } finally { _semaphore.Release(); }
        }

        #endregion
    }
}
