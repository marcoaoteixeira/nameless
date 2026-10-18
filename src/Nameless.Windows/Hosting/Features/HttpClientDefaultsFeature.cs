using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nameless.Application;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Configures the default <see cref="HttpClient"/> settings: the
///     <c>User-Agent</c> header with the application name and version.
/// </summary>
/// <remarks>
///     The defaults always apply; configuration added through
///     <see cref="WinHostFeature{TOptions}.Configure"/> extends them.
/// </remarks>
public class HttpClientDefaultsFeature : WinHostFeature<IHttpClientBuilder> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        // Captured now, like every other feature, so a later Configure
        // call has no effect.
        var configure = ConfigureAction;

        builder.ConfigureServices(services => services.ConfigureHttpClientDefaults(httpClient => {
            ConfigureDefaults(httpClient);
            configure?.Invoke(httpClient);
        }));
    }

    private static void ConfigureDefaults(IHttpClientBuilder builder) {
        builder.ConfigureHttpClient((provider, client) => {
            var applicationContext = provider.GetRequiredService<IApplicationContext>();
            var productHeaderValue = new ProductHeaderValue(applicationContext.ApplicationName, applicationContext.Version);
            var userAgent = new ProductInfoHeaderValue(productHeaderValue);

            client.DefaultRequestHeaders.UserAgent.Add(userAgent);
        });
    }
}
