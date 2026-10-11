using System.Net;
using Microsoft.Extensions.Time.Testing;
using Nameless.Auth.OAuth;
using Nameless.Auth.Support;

namespace Nameless.Auth;

public sealed class OAuthAccessTokenProviderTests
{
    private const string OkJson = """{"access_token":"abc","token_type":"Bearer"}""";

    private static readonly DateTimeOffset Start = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);
    private readonly FakeTimeProvider _time = new(Start);

    private OAuthAccessTokenProvider CreateProvider(
        StubHttpMessageHandler handler,
        Action<OAuthClientOptions>? tweak = null)
    {
        var options = new OAuthClientOptions
        {
            TokenEndpoint = new Uri("https://idp.example.com/oauth/token"),
            ClientId = "my-client",
            ClientSecret = "my-secret"
        };
        tweak?.Invoke(options);
        return new OAuthAccessTokenProvider(new StubHttpClientFactory(handler), "oauth", _time, options);
    }

    // ---------- Request ----------

    [Fact]
    public async Task ClientCredentials_posts_a_form_to_the_token_endpoint()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);

        await provider.GetTokenAsync(new ClientCredentialsRequest(Scope: "read write"), TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://idp.example.com/oauth/token", request.Uri?.ToString());
        Assert.Equal("application/x-www-form-urlencoded", request.ContentType);
        Assert.Contains("application/json", request.Accept);
        Assert.Equal("client_credentials", request.Form["grant_type"]);
        Assert.Equal("read write", request.Form["scope"]);
    }

    [Fact]
    public async Task ClientCredentials_omits_scope_and_audience_when_not_provided()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);

        await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        var form = Assert.Single(handler.Requests).Form;
        Assert.False(form.ContainsKey("scope"));
        Assert.False(form.ContainsKey("audience"));
    }

    [Fact]
    public async Task ClientCredentials_sends_audience_when_provided()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);

        await provider.GetTokenAsync(new ClientCredentialsRequest(Audience: "https://api.example.com"), TestContext.Current.CancellationToken);

        Assert.Equal("https://api.example.com", Assert.Single(handler.Requests).Form["audience"]);
    }

    [Fact]
    public async Task Different_requests_on_the_same_provider_send_different_parameters()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);

        await provider.GetTokenAsync(new ClientCredentialsRequest(Scope: "a"), TestContext.Current.CancellationToken);
        await provider.GetTokenAsync(new ClientCredentialsRequest(Scope: "b"), TestContext.Current.CancellationToken);

        Assert.Equal(["a", "b"], handler.Requests.Select(r => r.Form["scope"]));
    }

    [Fact]
    public async Task AuthorizationCode_sends_code_redirect_uri_and_code_verifier()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);

        await provider.GetTokenAsync(new AuthorizationCodeRequest("the-code", "https://app.example.com/cb", "the-verifier"), TestContext.Current.CancellationToken);

        var form = Assert.Single(handler.Requests).Form;
        Assert.Equal("authorization_code", form["grant_type"]);
        Assert.Equal("the-code", form["code"]);
        Assert.Equal("https://app.example.com/cb", form["redirect_uri"]);
        Assert.Equal("the-verifier", form["code_verifier"]);
    }

    [Fact]
    public async Task RefreshToken_sends_refresh_token_and_optional_scope()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);

        await provider.GetTokenAsync(new RefreshTokenRequest("old-refresh", "read"), TestContext.Current.CancellationToken);

        var form = Assert.Single(handler.Requests).Form;
        Assert.Equal("refresh_token", form["grant_type"]);
        Assert.Equal("old-refresh", form["refresh_token"]);
        Assert.Equal("read", form["scope"]);
    }

    [Fact]
    public async Task CustomGrant_sends_its_grant_type_and_parameters()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler);
        var request = new CustomGrantRequest(
            "urn:ietf:params:oauth:grant-type:jwt-bearer",
            new Dictionary<string, string> { ["assertion"] = "signed.jwt" });

        await provider.GetTokenAsync(request, TestContext.Current.CancellationToken);

        var form = Assert.Single(handler.Requests).Form;
        Assert.Equal("urn:ietf:params:oauth:grant-type:jwt-bearer", form["grant_type"]);
        Assert.Equal("signed.jwt", form["assertion"]);
    }

    // ---------- Client authentication ----------

    [Fact]
    public async Task Basic_authentication_url_encodes_credentials_before_base64()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler, o =>
        {
            o.ClientId = "my client";
            o.ClientSecret = "p@ss:w/rd";
        });

        await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        var expected = Convert.ToBase64String("my%20client:p%40ss%3Aw%2Frd"u8.ToArray());
        Assert.Equal($"Basic {expected}", request.Authorization);
        Assert.False(request.Form.ContainsKey("client_id"));
        Assert.False(request.Form.ContainsKey("client_secret"));
    }

    [Fact]
    public async Task Post_authentication_sends_credentials_in_the_body()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler, o => o.ClientAuthentication = ClientAuthenticationType.ClientSecretPost);

        await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Null(request.Authorization);
        Assert.Equal("my-client", request.Form["client_id"]);
        Assert.Equal("my-secret", request.Form["client_secret"]);
    }

    [Fact]
    public async Task Public_client_sends_only_the_client_id()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler, o =>
        {
            o.ClientAuthentication = ClientAuthenticationType.None;
            o.ClientSecret = null;
        });

        await provider.GetTokenAsync(new AuthorizationCodeRequest("c", "https://app.example.com/cb", "v"), TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Null(request.Authorization);
        Assert.Equal("my-client", request.Form["client_id"]);
        Assert.False(request.Form.ContainsKey("client_secret"));
    }

    [Fact]
    public async Task Client_authentication_wins_over_a_custom_parameter_with_the_same_name()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);
        var provider = CreateProvider(handler, o => o.ClientAuthentication = ClientAuthenticationType.ClientSecretPost);
        var request = new CustomGrantRequest("custom", new Dictionary<string, string> { ["client_id"] = "spoofed" });

        await provider.GetTokenAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal("my-client", Assert.Single(handler.Requests).Form["client_id"]);
    }

    // ---------- Successful response ----------

    [Fact]
    public async Task Parses_a_complete_token_response()
    {
        var handler = StubHttpMessageHandler.RespondingWith("""
            {"access_token":"abc","token_type":"Bearer","expires_in":3600,
             "scope":"a b","refresh_token":"rt","id_token":"idt"}
            """);
        var provider = CreateProvider(handler);

        var token = await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        Assert.Equal("abc", token.Value);
        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal(Start.AddHours(1), token.ExpiresAt);
        Assert.Equal("a b", token.Scope);
        Assert.Equal("rt", token.RefreshToken);
        Assert.NotNull(token.Extra);
        Assert.Equal("idt", token.Extra["id_token"].GetString());
        Assert.False(token.Extra.ContainsKey("access_token"));
    }

    [Fact]
    public async Task Accepts_expires_in_as_a_string()
    {
        var handler = StubHttpMessageHandler.RespondingWith("""{"access_token":"abc","expires_in":"120"}""");
        var provider = CreateProvider(handler);

        var token = await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        Assert.Equal(Start.AddSeconds(120), token.ExpiresAt);
    }

    [Fact]
    public async Task Defaults_token_type_to_Bearer_when_missing()
    {
        var handler = StubHttpMessageHandler.RespondingWith("""{"access_token":"abc"}""");
        var provider = CreateProvider(handler);

        var token = await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        Assert.Equal("Bearer", token.TokenType);
    }

    [Fact]
    public async Task Token_without_expires_in_has_no_expiration()
    {
        var handler = StubHttpMessageHandler.RespondingWith("""{"access_token":"abc"}""");
        var provider = CreateProvider(handler);

        var token = await provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken);

        Assert.Null(token.ExpiresAt);
    }

    [Theory]
    [InlineData("""{"token_type":"Bearer"}""")]
    [InlineData("""{"access_token":""}""")]
    [InlineData("""{"access_token":123}""")]
    [InlineData("""["access_token"]""")]
    public async Task Response_without_a_usable_access_token_throws(string json)
    {
        var provider = CreateProvider(StubHttpMessageHandler.RespondingWith(json));

        await Assert.ThrowsAsync<AccessTokenRequestException>(
            () => provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Malformed_json_on_success_is_reported_as_AccessTokenRequestException()
    {
        var provider = CreateProvider(StubHttpMessageHandler.RespondingWith("not json"));

        var ex = await Assert.ThrowsAsync<AccessTokenRequestException>(
            () => provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken).AsTask());

        Assert.IsType<System.Text.Json.JsonException>(ex.InnerException);
    }

    // ---------- Errors ----------

    [Fact]
    public async Task OAuth_error_response_exposes_error_fields()
    {
        var handler = StubHttpMessageHandler.RespondingWith(
            """{"error":"invalid_grant","error_description":"Code expired"}""", HttpStatusCode.BadRequest);
        var provider = CreateProvider(handler);

        var ex = await Assert.ThrowsAsync<AccessTokenRequestException>(
            () => provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken).AsTask());

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("invalid_grant", ex.Error);
        Assert.Equal("Code expired", ex.ErrorDescription);
    }

    [Fact]
    public async Task Non_json_error_body_falls_back_to_a_generic_error()
    {
        var provider = CreateProvider(new StubHttpMessageHandler(_ => Responses.Text("Bad Gateway", HttpStatusCode.BadGateway)));

        var ex = await Assert.ThrowsAsync<AccessTokenRequestException>(
            () => provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken).AsTask());

        Assert.Equal(502, ex.StatusCode);
        Assert.Null(ex.Error);
        Assert.Contains("Bad Gateway", ex.ErrorDescription);
    }

    [Fact]
    public async Task Network_failure_is_wrapped_in_AccessTokenRequestException()
    {
        var provider = CreateProvider(new StubHttpMessageHandler(_ => throw new HttpRequestException("boom")));

        var ex = await Assert.ThrowsAsync<AccessTokenRequestException>(
            () => provider.GetTokenAsync(new ClientCredentialsRequest(), TestContext.Current.CancellationToken).AsTask());

        Assert.IsType<HttpRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task Cancellation_is_not_wrapped()
    {
        var provider = CreateProvider(StubHttpMessageHandler.RespondingWith(OkJson));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetTokenAsync(new ClientCredentialsRequest(), cts.Token).AsTask());
    }

    // ---------- Options ----------

    [Fact]
    public void Missing_token_endpoint_is_rejected()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);

        Assert.Throws<InvalidOperationException>(() => new OAuthAccessTokenProvider(
            new StubHttpClientFactory(handler), "oauth", _time, new OAuthClientOptions { ClientId = "id", ClientSecret = "s" }));
    }

    [Fact]
    public void Missing_client_id_is_rejected()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);

        Assert.Throws<InvalidOperationException>(() => new OAuthAccessTokenProvider(
            new StubHttpClientFactory(handler), "oauth", _time, new OAuthClientOptions { TokenEndpoint = new Uri("https://idp.example.com/t"), ClientSecret = "s" }));
    }

    [Fact]
    public void Plain_http_is_rejected_for_remote_hosts()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);

        Assert.Throws<InvalidOperationException>(() => CreateProvider(handler, o => o.TokenEndpoint = new Uri("http://idp.example.com/t")));
    }

    [Fact]
    public void Plain_http_is_allowed_for_loopback()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);

        var provider = CreateProvider(handler, o => o.TokenEndpoint = new Uri("http://localhost:5000/t"));

        Assert.NotNull(provider);
    }

    [Fact]
    public void Client_secret_is_required_unless_the_client_is_public()
    {
        var handler = StubHttpMessageHandler.RespondingWith(OkJson);

        Assert.Throws<InvalidOperationException>(() => CreateProvider(handler, o => o.ClientSecret = null));
        Assert.NotNull(CreateProvider(handler, o =>
        {
            o.ClientSecret = null;
            o.ClientAuthentication = ClientAuthenticationType.None;
        }));
    }

    [Fact]
    public void Negative_clock_skew_is_rejected()
    {
        var options = new OAuthClientOptions();

        Assert.Throws<ArgumentOutOfRangeException>(() => options.ClockSkew = TimeSpan.FromSeconds(-1));
    }
}
