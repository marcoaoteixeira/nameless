using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Configuration;
using Nameless.ObjectModel;
using Nameless.Testing.Tools.Mockers.Logging;

namespace Nameless.GitHub;

[UnitTest]
public class GitHubHttpClientTests {
    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler {
        public string? LastUrl { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            LastUrl = request.RequestUri?.ToString();

            return Task.FromResult(respond(request));
        }
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) {
        return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
    }

    private const string AuthorJson = """
                                      {"login":"l","id":1,"node_id":"n","avatar_url":"a","gravatar_id":"g","url":"u","html_url":"h","followers_url":"f",
                                       "following_url":"f","gists_url":"g","starred_url":"s","subscriptions_url":"s","organizations_url":"o","repos_url":"r",
                                       "events_url":"e","received_events_url":"r","type":"User","user_view_type":"public","site_admin":false}
                                      """;

    private static string ReleaseJson => $$"""
        {"url":"u","assets_url":"a","upload_url":"up","html_url":"h","id":7,"author":{{AuthorJson}},"node_id":"n","tag_name":"v1.2.3",
         "target_commitish":"main","name":"Release","draft":false,"immutable":false,"prerelease":false,
         "created_at":"2026-01-01T00:00:00Z","updated_at":"2026-01-01T00:00:00Z","published_at":"2026-01-01T00:00:00Z",
         "assets":[],"tarball_url":"t","zipball_url":"z","body":"notes"}
        """;

    private static string AssetsJson => $$"""
        [{"url":"u","id":1,"node_id":"n","name":"app.zip","label":"l","uploader":{{AuthorJson}},"content_type":"application/zip",
          "state":"uploaded","size":10,"digest":null,"download_count":2,"created_at":"2026-01-01T00:00:00Z",
          "updated_at":"2026-01-01T00:00:00Z","browser_download_url":"b"}]
        """;

    private static (GitHubHttpClient Sut, StubHandler Handler) CreateSut(Func<HttpRequestMessage, HttpResponseMessage> respond) {
        var handler = new StubHandler(respond);
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.test") };
        var logger = new LoggerMocker<GitHubHttpClient>().WithAnyLogLevel().Build();

        return (new GitHubHttpClient(client, logger), handler);
    }

    // ── latest release ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetLastestReleaseAsync_WithSuccess_ReturnsRelease() {
        // arrange
        var (sut, handler) = CreateSut(_ => Json(ReleaseJson));

        // act
        var response = await sut.GetLastestReleaseAsync("owner", "repo", TestContext.Current.CancellationToken);

        // assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Equal("v1.2.3", response.Value.TagName),
            () => Assert.EndsWith("/repos/owner/repo/releases/latest", handler.LastUrl)
        );
    }

    [Fact]
    public async Task GetLastestReleaseAsync_WithHttpError_ReturnsFailureWithStatusCode() {
        // arrange
        var (sut, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        // act
        var response = await sut.GetLastestReleaseAsync("o", "r", TestContext.Current.CancellationToken);

        // assert
        Assert.Multiple(
            () => Assert.False(response.Success),
            () => Assert.Equal(ErrorType.Failure, response.Errors[0].Type),
            () => Assert.Contains("404", response.Errors[0].Message)
        );
    }

    [Fact]
    public async Task GetLastestReleaseAsync_WithNullBody_ReturnsConflict() {
        // arrange
        var (sut, _) = CreateSut(_ => Json("null"));

        // act
        var response = await sut.GetLastestReleaseAsync("o", "r", TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(ErrorType.Conflict, response.Errors[0].Type);
    }

    [Fact]
    public async Task GetLastestReleaseAsync_WhenHandlerThrows_ReturnsFailure() {
        // arrange
        var (sut, _) = CreateSut(_ => throw new HttpRequestException("network"));

        // act
        var response = await sut.GetLastestReleaseAsync("o", "r", TestContext.Current.CancellationToken);

        // assert
        Assert.Multiple(
            () => Assert.False(response.Success),
            () => Assert.Contains("network", response.Errors[0].Message)
        );
    }

    // ── release assets ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReleaseAssetsAsync_WithSuccess_ReturnsAssets() {
        // arrange
        var (sut, handler) = CreateSut(_ => Json(AssetsJson));

        // act
        var response = await sut.GetReleaseAssetsAsync("owner", "repo", 7, TestContext.Current.CancellationToken);

        // assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Equal("app.zip", response.Value.Single().Name),
            () => Assert.EndsWith("/repos/owner/repo/releases/7/assets", handler.LastUrl)
        );
    }

    [Fact]
    public async Task GetReleaseAssetsAsync_WithHttpError_ReturnsFailure() {
        // arrange
        var (sut, _) = CreateSut(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        // act
        var response = await sut.GetReleaseAssetsAsync("o", "r", 1, TestContext.Current.CancellationToken);

        // assert
        Assert.Contains("500", response.Errors[0].Message);
    }

    [Fact]
    public async Task GetReleaseAssetsAsync_WithNullBody_ReturnsConflict() {
        // arrange
        var (sut, _) = CreateSut(_ => Json("null"));

        // act
        var response = await sut.GetReleaseAssetsAsync("o", "r", 1, TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(ErrorType.Conflict, response.Errors[0].Type);
    }

    // ── responses ─────────────────────────────────────────────────────────────

    [Fact]
    public void Responses_ImplicitOperators_BuildSuccessAndFailure() {
        // act
        GetLastestReleaseResult failedRelease = Error.Failure("x");
        GetReleaseAssetsResult failedAssets = Error.Failure("y");
        GetReleaseAssetsResult okAssets = Array.Empty<ObjectModel.ReleaseAsset>();

        // assert
        Assert.Multiple(
            () => Assert.False(failedRelease.Success),
            () => Assert.False(failedAssets.Success),
            () => Assert.True(okAssets.Success)
        );
    }

    // ── registration ──────────────────────────────────────────────────────────

    [Fact]
    public void RegisterGitHubHttpClient_WithDelegate_ResolvesClient() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // act
        services.RegisterGitHubHttpClient(options => options.Owner = "me");
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.IsType<GitHubHttpClient>(provider.GetRequiredService<IGitHubHttpClient>());
    }

    [Fact]
    public void RegisterGitHubHttpClient_WithoutConfigure_ResolvesClient() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // act
        services.RegisterGitHubHttpClient();
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.NotNull(provider.GetRequiredService<IGitHubHttpClient>());
    }

    [Fact]
    public void RegisterGitHubHttpClient_WithConfiguration_BindsOptions() {
        // arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["GitHub:ApiBaseUrl"] = "https://ghe.local" })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();

        // act
        services.RegisterGitHubHttpClient(configuration);
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.NotNull(provider.GetRequiredService<IGitHubHttpClient>());
    }

    [Fact]
    public void RegisterGitHubHttpClient_WithBlankBaseUrl_ThrowsMissingConfigurationException() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.RegisterGitHubHttpClient(options => options.ApiBaseUrl = " ");
        using var provider = services.BuildServiceProvider();

        // act & assert
        Assert.Throws<MissingConfigurationException>(provider.GetRequiredService<IGitHubHttpClient>);
    }
}
