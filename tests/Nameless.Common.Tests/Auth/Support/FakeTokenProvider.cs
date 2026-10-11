namespace Nameless.Auth.Support;

/// <summary>Fake provider that counts calls and lets the test control what it returns.</summary>
internal sealed class FakeTokenProvider : IAccessTokenProvider<string>
{
    private readonly Func<string, int, CancellationToken, ValueTask<AccessToken>> _factory;
    private readonly List<string> _invalidated = [];
    private int _calls;

    public FakeTokenProvider(Func<string, int, CancellationToken, ValueTask<AccessToken>> factory)
    {
        _factory = factory;
    }

    public int Calls => Volatile.Read(ref _calls);

    public IReadOnlyList<string> Invalidated => _invalidated;

    /// <summary>Returns "tok-1", "tok-2", ... with the given lifetime (or no expiration).</summary>
    public static FakeTokenProvider Sequential(TimeProvider time, TimeSpan? lifetime = null)
    {
        return new FakeTokenProvider((_, n, _) => ValueTask.FromResult(new AccessToken(
            $"tok-{n}",
            ExpiresAt: lifetime is { } l ? time.GetUtcNow() + l : null)));
    }

    public async ValueTask<AccessToken> GetTokenAsync(string request, CancellationToken cancellationToken = default)
    {
        var call = Interlocked.Increment(ref _calls);
        return await _factory(request, call, cancellationToken);
    }

    public void Invalidate(string request)
    {
        _invalidated.Add(request);
    }
}
