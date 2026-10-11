using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Configuration;

namespace Nameless.GitHub;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class GitHubServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Register GitHub HTTP client.
        /// </summary>
        /// <param name="configure">
        ///     The configuration action.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterGitHubHttpClient(Action<GitHubOptions>? configure = null) {
            return self.Configure(configure ?? (_ => { }))
                       .RegisterGitHubClientCore();
        }

        /// <summary>
        ///     Register GitHub HTTP client.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterGitHubHttpClient(IConfiguration configuration) {
            return self.ConfigureOptions<GitHubOptions>(configuration)
                       .RegisterGitHubClientCore();
        }

        private IServiceCollection RegisterGitHubClientCore() {
            self.AddHttpClient<IGitHubHttpClient, GitHubHttpClient>((provider, client) => {
                var opts = provider.GetOptions<GitHubOptions>().Value;

                if (string.IsNullOrWhiteSpace(opts.ApiBaseUrl)) {
                    throw new MissingConfigurationException(
                        section: ConfigurationSectionNameAttribute.GetSectionName<GitHubOptions>(),
                        key: nameof(GitHubOptions.ApiBaseUrl));
                }

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", [opts.ApiVersion]);
                client.BaseAddress = new Uri(opts.ApiBaseUrl);
            });

            return self;
        }
    }
}
