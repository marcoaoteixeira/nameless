using Microsoft.Extensions.Time.Testing;
using Nameless.Auth.Support;

namespace Nameless.Auth;

public sealed class CachingAccessTokenProviderTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);
    private readonly FakeTimeProvider _time = new(Start);

    [Fact]
    public async Task Second_call_returns_the_cached_token()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);

        var first = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        var second = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        Assert.Same(first, second);
        Assert.Equal(1, inner.Calls);
    }

    [Fact]
    public async Task Token_is_renewed_once_inside_the_clock_skew_window()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time); // default clock skew: 60 s

        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        _time.Advance(TimeSpan.FromMinutes(8));
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        Assert.Equal(1, inner.Calls);

        _time.Advance(TimeSpan.FromMinutes(1)); // 9 min = expiration - clock skew
        var renewed = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        Assert.Equal(2, inner.Calls);
        Assert.Equal("tok-2", renewed.Value);
    }

    [Fact]
    public async Task Custom_clock_skew_is_honoured()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time, clockSkew: TimeSpan.FromMinutes(5));

        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        _time.Advance(TimeSpan.FromMinutes(5));
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task Token_without_expiration_is_cached_indefinitely()
    {
        var inner = FakeTokenProvider.Sequential(_time, lifetime: null);
        var cache = new CachingAccessTokenProvider<string>(inner, _time);

        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        _time.Advance(TimeSpan.FromDays(365));
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        Assert.Equal(1, inner.Calls);
    }

    [Fact]
    public async Task Different_requests_are_cached_separately()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);

        var a = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        var b = await cache.GetTokenAsync("b", TestContext.Current.CancellationToken);
        var aAgain = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        Assert.NotEqual(a.Value, b.Value);
        Assert.Same(a, aAgain);
        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task Concurrent_calls_for_the_same_request_share_one_fetch()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var inner = new FakeTokenProvider(async (_, n, _) =>
        {
            entered.TrySetResult();
            await release.Task;
            return new AccessToken($"tok-{n}", ExpiresAt: _time.GetUtcNow().AddMinutes(10));
        });
        var cache = new CachingAccessTokenProvider<string>(inner, _time);

        var calls = Enumerable.Range(0, 10).Select(_ => cache.GetTokenAsync("a", TestContext.Current.CancellationToken).AsTask()).ToArray();
        await entered.Task;
        release.SetResult();
        var results = await Task.WhenAll(calls);

        Assert.Equal(1, inner.Calls);
        Assert.All(results, r => Assert.Same(results[0], r));
    }

    [Fact]
    public async Task Invalidate_forces_a_new_fetch_and_notifies_the_inner_provider()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        cache.Invalidate("a");
        var renewed = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        Assert.Equal("tok-2", renewed.Value);
        Assert.Equal(["a"], inner.Invalidated);
    }

    [Fact]
    public async Task Invalidate_only_affects_the_given_request()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        await cache.GetTokenAsync("b", TestContext.Current.CancellationToken);

        cache.Invalidate("a");
        await cache.GetTokenAsync("b", TestContext.Current.CancellationToken);

        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task Failures_are_not_cached()
    {
        var inner = new FakeTokenProvider((_, n, _) => n == 1
            ? throw new AccessTokenRequestException("boom")
            : ValueTask.FromResult(new AccessToken("ok")));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);

        await Assert.ThrowsAsync<AccessTokenRequestException>(() => cache.GetTokenAsync("a", TestContext.Current.CancellationToken).AsTask());
        Assert.Equal(0, cache.EntryCount); // a failed entry must not keep occupying memory

        var token = await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        Assert.Equal("ok", token.Value);
        Assert.Equal(2, inner.Calls);
    }

    [Fact]
    public async Task Expired_entries_of_other_requests_are_evicted_on_the_next_fetch()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);
        _time.Advance(TimeSpan.FromMinutes(30));

        await cache.GetTokenAsync("b", TestContext.Current.CancellationToken);

        Assert.Equal(1, cache.EntryCount);
    }

    [Fact]
    public async Task Unexpired_entries_are_kept_when_fetching_another_request()
    {
        var inner = FakeTokenProvider.Sequential(_time, TimeSpan.FromMinutes(10));
        var cache = new CachingAccessTokenProvider<string>(inner, _time);
        await cache.GetTokenAsync("a", TestContext.Current.CancellationToken);

        await cache.GetTokenAsync("b", TestContext.Current.CancellationToken);

        Assert.Equal(2, cache.EntryCount);
    }

    [Fact]
    public async Task Cancelling_while_waiting_for_an_in_flight_fetch_does_not_cancel_the_fetch()
    {
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var inner = new FakeTokenProvider(async (_, n, _) =>
        {
            entered.TrySetResult();
            await release.Task;
            return new AccessToken($"tok-{n}", ExpiresAt: _time.GetUtcNow().AddMinutes(10));
        });
        var cache = new CachingAccessTokenProvider<string>(inner, _time);
        var first = cache.GetTokenAsync("a", TestContext.Current.CancellationToken).AsTask();
        await entered.Task;
        using var cts = new CancellationTokenSource();

        var waiter = cache.GetTokenAsync("a", cts.Token).AsTask();
        await cts.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiter);
        release.SetResult();

        Assert.Equal("tok-1", (await first).Value);
    }
}
