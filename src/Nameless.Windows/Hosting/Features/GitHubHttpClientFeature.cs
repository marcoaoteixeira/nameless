using Nameless.GitHub;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the GitHub HTTP client.
/// </summary>
public class GitHubHttpClientFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            (ctx, services) => services.RegisterGitHubHttpClient(ctx.Configuration)
        );
    }
}
