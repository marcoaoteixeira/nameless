using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Auth.OAuth;
using Nameless.Helpers;

namespace Nameless.Auth;

/// <summary>
///     Auth <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class AuthServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/> instance.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers any provider, wrapped in a cache.
        /// </summary>
        /// <typeparam name="TRequest">
        ///     Token request type of the provider.
        /// </typeparam>
        /// <param name="name">
        ///     Name (key) of the source.
        /// </param>
        /// <param name="factory">
        ///     Creates the provider; the source's HTTP client is available in
        ///     the factory under the name <see cref="HttpClientNameFor"/>.
        /// </param>
        /// <param name="clockSkew">
        ///     Margin before expiration; defaults to 60 s.
        /// </param>
        /// <param name="comparer">
        ///     Comparer for the cache keys; defaults to the equality of
        ///     <typeparamref name="TRequest"/>.
        /// </param>
        /// <returns>
        ///     Builder of the source's HTTP client.
        /// </returns>
        public IHttpClientBuilder RegisterCachingAccessTokenProvider<TRequest>(string name, Func<IServiceProvider, IHttpClientFactory, TimeProvider, IAccessTokenProvider<TRequest>> factory, TimeSpan? clockSkew = null, IEqualityComparer<TRequest>? comparer = null) where TRequest : notnull {
            Throws.When.NullOrWhiteSpace(name);
            Throws.When.Null(factory);

            self.TryAddSingleton(TimeProvider.System);

            // Singleton: the cache must be shared by all consumers to avoid
            // duplicate requests.
            self.AddKeyedSingleton<IAccessTokenProvider<TRequest>>(name, (provider, _) =>
            {
                var time = provider.GetRequiredService<TimeProvider>();
                var inner = factory(provider, provider.GetRequiredService<IHttpClientFactory>(), time);

                return new CachingAccessTokenProvider<TRequest>(inner, time, clockSkew, comparer);
            });

            return self.AddHttpClient(
                HttpClientNameFor(name)
            );
        }

        /// <summary>
        ///     Registers a cached OAuth 2.0 provider.
        /// </summary>
        /// <param name="name">
        ///     Name (key) of the source.
        /// </param>
        /// <param name="configure">
        ///     Configures the OAuth client.
        /// </param>
        /// <returns>
        ///     Builder of the HTTP client for the Token Endpoint.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     The options are invalid; fails at registration rather than on
        ///     first use.
        /// </exception>
        /// <remarks>
        ///     Resolve with <![CDATA[GetRequiredKeyedService<IAuthorizationTokenProvider<OAuthTokenRequest>>(name)]]>.
        /// </remarks>
        public IHttpClientBuilder RegisterOAuthAccessTokenProvider(string name, Action<OAuthClientOptions>? configure = null) {
            Throws.When.NullOrWhiteSpace(name);
            Throws.When.Null(configure);

            // The options are read at registration time so configuration
            // errors show up at startup.
            var options = ActionHelper.FromDelegate(configure)
                                      .Validate();

            return self.RegisterCachingAccessTokenProvider(
                name: name,
                factory: (_, httpClientFactory, timeProvider) => new OAuthAccessTokenProvider(
                    httpClientFactory,
                    HttpClientNameFor(name),
                    timeProvider,
                    options
                ),
                clockSkew: options.ClockSkew
            );
        }
    }

    /// <summary>
    ///     Name of the HTTP client associated with a token source.
    /// </summary>
    /// <param name="name">
    ///     Name of the source.
    /// </param>
    /// <returns>
    ///     The client name in the <see cref="IHttpClientFactory"/>.
    /// </returns>
    internal static string HttpClientNameFor(string name) {
        // The prefix avoids collisions with application clients that use the
        // same name.
        return $"token-source:{name}";
    }
}
