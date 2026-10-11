using Microsoft.Extensions.DependencyInjection;
using Nameless.Auth.OAuth;
using Nameless.Auth.Support;

namespace Nameless.Auth;

public sealed class OAuthServiceCollectionExtensionsTests
{
    private const string TokenJson = """{"access_token":"tok","token_type":"Bearer","expires_in":3600}""";

    private static void ConfigureOAuth(OAuthClientOptions o)
    {
        o.TokenEndpoint = new Uri("https://idp.example.com/oauth/token");
        o.ClientId = "id";
        o.ClientSecret = "secret";
    }

    [Fact]
    public async Task RegisterOAuthAccessTokenProvider_registers_a_cached_keyed_provider()
    {
        var idp = StubHttpMessageHandler.RespondingWith(TokenJson);
        var services = new ServiceCollection();
        services.RegisterOAuthAccessTokenProvider("crm", ConfigureOAuth)
                .ConfigurePrimaryHttpMessageHandler(() => idp);
        await using var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredKeyedService<IAccessTokenProvider<OAuthTokenRequest>>("crm");
        var first = await provider.GetTokenAsync(new ClientCredentialsRequest("read"), TestContext.Current.CancellationToken);
        var second = await provider.GetTokenAsync(new ClientCredentialsRequest("read"), TestContext.Current.CancellationToken);

        Assert.Equal("tok", first.Value);
        Assert.Same(first, second);
        Assert.Single(idp.Requests);
    }

    [Fact]
    public void Provider_is_a_singleton_so_the_cache_is_shared()
    {
        var services = new ServiceCollection();
        services.RegisterOAuthAccessTokenProvider("crm", ConfigureOAuth);
        using var sp = services.BuildServiceProvider();

        var a = sp.GetRequiredKeyedService<IAccessTokenProvider<OAuthTokenRequest>>("crm");
        var b = sp.GetRequiredKeyedService<IAccessTokenProvider<OAuthTokenRequest>>("crm");

        Assert.Same(a, b);
    }

    [Fact]
    public void Invalid_oauth_options_fail_at_registration_not_at_first_use()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.RegisterOAuthAccessTokenProvider("crm", _ => { }));
    }

    [Fact]
    public async Task Named_sources_do_not_share_tokens()
    {
        var idpA = StubHttpMessageHandler.RespondingWith("""{"access_token":"from-a"}""");
        var idpB = StubHttpMessageHandler.RespondingWith("""{"access_token":"from-b"}""");
        var services = new ServiceCollection();
        services.RegisterOAuthAccessTokenProvider("a", ConfigureOAuth).ConfigurePrimaryHttpMessageHandler(() => idpA);
        services.RegisterOAuthAccessTokenProvider("b", ConfigureOAuth).ConfigurePrimaryHttpMessageHandler(() => idpB);
        await using var sp = services.BuildServiceProvider();

        var a = await sp.GetRequiredKeyedService<IAccessTokenProvider<OAuthTokenRequest>>("a")
            .GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);
        var b = await sp.GetRequiredKeyedService<IAccessTokenProvider<OAuthTokenRequest>>("b")
            .GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        Assert.Equal("from-a", a.Value);
        Assert.Equal("from-b", b.Value);
    }

    [Fact]
    public async Task AddCachedTokenProvider_accepts_any_custom_provider()
    {
        var services = new ServiceCollection();
        services.RegisterCachingAccessTokenProvider("custom",
            (_, _, time) => FakeTokenProvider.Sequential(time, TimeSpan.FromMinutes(10)));
        await using var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredKeyedService<IAccessTokenProvider<string>>("custom");
        var first = await provider.GetTokenAsync("x", TestContext.Current.CancellationToken);
        var second = await provider.GetTokenAsync("x", TestContext.Current.CancellationToken);

        Assert.Same(first, second);
    }
}
