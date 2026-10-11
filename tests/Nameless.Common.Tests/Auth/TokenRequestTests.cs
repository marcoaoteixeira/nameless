using Nameless.Auth.OAuth;

namespace Nameless.Auth;

public sealed class TokenRequestTests
{
    [Fact]
    public void ToString_never_includes_secret_values()
    {
        var texts = new[]
        {
            new RefreshTokenRequest("secret-refresh").ToString(),
            new AuthorizationCodeRequest("secret-code", "https://app.example.com/cb", "secret-verifier").ToString(),
            new CustomGrantRequest("custom", new Dictionary<string, string> { ["assertion"] = "secret-assertion" }).ToString()
        };

        Assert.All(texts, text => Assert.DoesNotContain("secret", text));
    }

    [Fact]
    public void Requests_with_the_same_values_are_equal_so_the_cache_can_key_on_them()
    {
        Assert.Equal(new ClientCredentialsRequest("a", "b"), new ClientCredentialsRequest("a", "b"));
        Assert.NotEqual(new ClientCredentialsRequest("a"), new ClientCredentialsRequest("b"));
    }

    [Fact]
    public void Requests_of_different_grants_are_never_equal()
    {
        OAuthTokenRequest credentials = new ClientCredentialsRequest("read");
        OAuthTokenRequest refresh = new RefreshTokenRequest("read");

        Assert.NotEqual(credentials, refresh);
    }

    [Fact]
    public void CustomGrant_equality_ignores_parameter_order()
    {
        var first = new CustomGrantRequest("g", new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" });
        var second = new CustomGrantRequest("g", new Dictionary<string, string> { ["b"] = "2", ["a"] = "1" });

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void CustomGrant_with_different_values_is_not_equal()
    {
        var first = new CustomGrantRequest("g", new Dictionary<string, string> { ["a"] = "1" });
        var second = new CustomGrantRequest("g", new Dictionary<string, string> { ["a"] = "2" });
        var third = new CustomGrantRequest("other", new Dictionary<string, string> { ["a"] = "1" });

        Assert.NotEqual(first, second);
        Assert.NotEqual(first, third);
    }

    [Fact]
    public void CustomGrant_is_isolated_from_later_changes_to_the_source_dictionary()
    {
        var source = new Dictionary<string, string> { ["a"] = "1" };
        var request = new CustomGrantRequest("g", source);
        var hashBefore = request.GetHashCode();

        source["a"] = "changed";

        Assert.Equal("1", request.Parameters["a"]);
        Assert.Equal(hashBefore, request.GetHashCode());
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void CustomGrant_requires_a_grant_type(string grantType)
    {
        Assert.Throws<ArgumentException>(() => new CustomGrantRequest(grantType));
    }
}
