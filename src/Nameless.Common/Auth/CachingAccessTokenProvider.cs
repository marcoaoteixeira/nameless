using System.Collections.Concurrent;

namespace Nameless.Auth;

/// <summary>
///     Stores tokens per request and only queries the source when a token
///     is missing or about to expire.
/// </summary>
/// <typeparam name="TRequest">
///     Parameters of the token request; used as the cache key.
/// </typeparam>
/// <remarks>
///     Concurrent equal requests share a single query to the source
///     (single-flight), which avoids request bursts when a token
///     expires under load.
/// </remarks>
public sealed class CachingAccessTokenProvider<TRequest> : IAccessTokenProvider<TRequest> where TRequest : notnull {
    private readonly IAccessTokenProvider<TRequest> _inner;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _clockSkew;
    private readonly ConcurrentDictionary<TRequest, CacheEntry> _entries;

    /// <summary>
    ///     Number of entries held in memory; exists so tests can
    ///     verify cleanup.
    /// </summary>
    internal int EntryCount => _entries.Count;

    /// <summary>Creates the cache around a provider.</summary>
    /// <param name="inner">Provider queried when there is no valid token in the cache.</param>
    /// <param name="timeProvider">Source of the current instant.</param>
    /// <param name="clockSkew">Margin before expiration in which a token is already considered expired. Defaults to 60 s.</param>
    /// <param name="comparer">Key comparer; defaults to the equality of <typeparamref name="TRequest"/> itself.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="clockSkew"/> is negative.</exception>
    public CachingAccessTokenProvider(IAccessTokenProvider<TRequest> inner, TimeProvider timeProvider, TimeSpan? clockSkew = null, IEqualityComparer<TRequest>? comparer = null) {
        Throws.When.Null(inner);
        Throws.When.Null(timeProvider);

        if (clockSkew.HasValue) {
            Throws.When.LowerThan(clockSkew.Value, paramName: nameof(clockSkew), compare: TimeSpan.Zero, message: "Clock skew cannot be negative.");
        }

        _inner = inner;
        _timeProvider = timeProvider;
        _clockSkew = clockSkew ?? TimeSpan.FromSeconds(60);
        _entries = new ConcurrentDictionary<TRequest, CacheEntry>(comparer);
    }

    /// <inheritdoc />
    public async ValueTask<AccessToken> GetTokenAsync(TRequest request, CancellationToken cancellationToken = default) {
        Throws.When.Null(request);

        var entry = _entries.GetOrAdd(request, static _ => new CacheEntry());

        // Lock-free fast path: it covers almost every call.
        if (entry.Token is { } cached && !cached.IsExpired(_timeProvider, _clockSkew)) {
            return cached;
        }

        await entry.Gate.WaitAsync(cancellationToken).SkipContextSync();

        try {
            // Callers that were waiting may find the token already
            // renewed by another caller.
            if (entry.Token is { } renewed && !renewed.IsExpired(_timeProvider, _clockSkew)) {
                return renewed;
            }

            EvictExpiredEntries();

            // The query uses the cancellation token of whoever holds the
            // gate. If that caller cancels, the callers that were waiting
            // take over the query instead of getting stuck.
            var token = await _inner.GetTokenAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

            entry.Token = token;
            
            return token;
        }
        catch when (entry.Token is null) {
            // Without a token the entry only occupies memory (e.g.
            // authorization codes that failed).
            _entries.TryRemove(new KeyValuePair<TRequest, CacheEntry>(request, entry));

            throw;
        }
        finally { entry.Gate.Release(); }
    }

    /// <inheritdoc />
    public void Invalidate(TRequest request) {
        Throws.When.Null(request);

        if (_entries.TryGetValue(request, out var entry)) {
            entry.Token = null;
        }

        _inner.Invalidate(request);
    }

    /// <summary>
    ///     Removes entries whose token has already expired, so the cache does
    ///     not grow without bound when requests vary (e.g. a different
    ///     authorization code on every call).
    /// </summary>
    private void EvictExpiredEntries() {
        foreach (var pair in _entries) {
            // Entries without a token or with a query in flight (busy Gate)
            // may belong to another caller in the middle of its first
            // request, and removing them would break single-flight.
            if (pair.Value.Token is { } token &&
                token.IsExpired(_timeProvider, TimeSpan.Zero) &&
                pair.Value.Gate.CurrentCount == 1) { _entries.TryRemove(pair); }
        }
    }

    private sealed class CacheEntry {
        // The semaphore is not disposed: a removed entry may still have
        // callers waiting on it, and disposing it would make those callers
        // fail. It holds no native resources unless AvailableWaitHandle is
        // used, which it never is.
        public SemaphoreSlim Gate { get; } = new(1, 1);

        public volatile AccessToken? Token;
    }
}

