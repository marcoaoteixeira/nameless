using Nameless.Configuration;

namespace Nameless.Auth.OAuth;

/// <summary>
///     Stable configuration of the OAuth client. Anything that varies between
///     requests (grant, scope, code, ...) goes in
///     <see cref="OAuthTokenRequest"/>.
/// </summary>
[ConfigurationSectionName("OAuthClient")]
public sealed class OAuthClientOptions {
    /// <summary>
    ///     URL of the Token Endpoint. Must be HTTPS, except for loopback
    ///     addresses.
    /// </summary>
    public Uri TokenEndpoint { get; set; } = null!;

    /// <summary>
    ///     Client identifier.
    /// </summary>
    public string ClientId { get; set; } = null!;

    /// <summary>
    ///     Client secret. Required unless <see cref="ClientAuthentication"/>
    ///     is <see cref="ClientAuthenticationType.None"/>.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    ///     Client authentication method. Defaults to
    ///     <see cref="ClientAuthenticationType.ClientSecretBasic"/>.
    /// </summary>
    public ClientAuthenticationType ClientAuthentication { get; set; } = ClientAuthenticationType.ClientSecretBasic;

    /// <summary>
    ///     Margin before expiration at which the cache renews the token.
    ///     Defaults to 60 s.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value is negative.
    /// </exception>
    public TimeSpan ClockSkew {
        get;
        set => field = Throws.When.LowerThan(value, compare: TimeSpan.Zero, message: "Clock skew cannot be negative.");
    } = TimeSpan.FromSeconds(60);

    /// <summary>
    ///     Checks that the configuration is complete and secure.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     A required value is missing or the endpoint uses insecure HTTP.
    /// </exception>
    internal OAuthClientOptions Validate() {
        if (TokenEndpoint is null || !TokenEndpoint.IsAbsoluteUri) {
            throw new InvalidOperationException(
                $"{nameof(TokenEndpoint)} must be an absolute URI."
            );
        }

        // The client secret and the tokens travel through this endpoint;
        // plain HTTP only makes sense in local development.
        if (TokenEndpoint.Scheme != Uri.UriSchemeHttps && !TokenEndpoint.IsLoopback) {
            throw new InvalidOperationException(
                $"{nameof(TokenEndpoint)} must use HTTPS (HTTP is only allowed for loopback addresses)."
            );
        }

        if (string.IsNullOrWhiteSpace(ClientId)) {
            throw new InvalidOperationException(
                $"{nameof(ClientId)} is required."
            );
        }

        if (ClientAuthentication != ClientAuthenticationType.None && string.IsNullOrEmpty(ClientSecret)) {
            throw new InvalidOperationException(
                $"{nameof(ClientSecret)} is required unless {nameof(ClientAuthentication)} is {nameof(ClientAuthenticationType.None)}."
            );
        }

        return this;
    }
}