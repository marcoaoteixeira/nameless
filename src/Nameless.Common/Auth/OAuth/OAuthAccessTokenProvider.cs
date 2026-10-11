using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Nameless.Auth.OAuth;

/// <summary>
///     Gets tokens from an OAuth 2.0 Token Endpoint (RFC 6749 §3.2 and §5).
/// </summary>
public sealed class OAuthAccessTokenProvider : AccessTokenProviderBase<OAuthTokenRequest> {
    // Fields that already have a dedicated place in AccessToken;
    // the rest go to Extra.
    private static readonly HashSet<string> KnownFields = [
        "access_token", 
        "token_type", 
        "expires_in", 
        "scope", 
        "refresh_token"
    ];

    private readonly OAuthClientOptions _options;

    /// <summary>
    ///     Initializes a new instance of
    ///     <see cref="OAuthAccessTokenProvider"/> class.
    /// </summary>
    /// <param name="httpClientFactory">
    ///     HTTP client factory.
    /// </param>
    /// <param name="httpClientName">
    ///     Name of the HTTP client used to talk to the Token Endpoint.
    /// </param>
    /// <param name="timeProvider">
    ///     Source of the current instant.
    /// </param>
    /// <param name="options">
    ///     OAuth client configuration.
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     The options are incomplete or insecure.
    /// </exception>
    public OAuthAccessTokenProvider(IHttpClientFactory httpClientFactory, string httpClientName, TimeProvider timeProvider, OAuthClientOptions options) : base(httpClientFactory, httpClientName, timeProvider) {
        _options = Throws.When.Null(options).Validate();
    }

    /// <inheritdoc />
    protected override HttpRequestMessage CreateRequest(OAuthTokenRequest request) {
        var form = new Dictionary<string, string>(StringComparer.Ordinal) {
            ["grant_type"] = request.GrantType
        };

        request.WriteParameters(form);

        var message = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);

        message.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        // When client_id/client_secret go in the body, they are written after
        // the request parameters so a CustomGrantRequest cannot override
        // them.
        switch (_options.ClientAuthentication) {
            case ClientAuthenticationType.ClientSecretBasic:
                // RFC 6749 §2.3.1 says to encode the id and secret before
                // Base64. Spaces use %20 (not '+') because that is accepted
                // by servers with and without form decoding.
                var credentials = $"{Uri.EscapeDataString(_options.ClientId)}:{Uri.EscapeDataString(_options.ClientSecret!)}";

                message.Headers.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials))
                );
                break;

            case ClientAuthenticationType.ClientSecretPost:
                form["client_id"] = _options.ClientId;
                form["client_secret"] = _options.ClientSecret!;
                break;

            case ClientAuthenticationType.None:
                form["client_id"] = _options.ClientId;
                break;
        }

        // FormUrlEncodedContent expects nullable pairs; the conversion avoids
        // nullability warnings.
        message.Content = new FormUrlEncodedContent(
            form.Select(param => new KeyValuePair<string?, string?>(param.Key, param.Value))
        );

        return message;
    }

    /// <inheritdoc />
    /// <exception cref="AccessTokenRequestException">
    ///     The body is not valid JSON or does not contain <c>access_token</c>.
    /// </exception>
    protected override async ValueTask<AccessToken> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
        try {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).SkipContextSync();
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).SkipContextSync();

            return ReadToken(document.RootElement, (int)response.StatusCode);
        }
        catch (JsonException ex) {
            throw new AccessTokenRequestException(
                message: "Token response is not valid JSON.",
                statusCode: (int)response.StatusCode,
                inner: ex
            );
        }
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Interprets the RFC 6749 §5.2 error format; if the body does not
    ///     follow it, falls back to the generic handling.
    /// </remarks>
    protected override async ValueTask<AccessTokenRequestException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).SkipContextSync();
        var statusCode = (int)response.StatusCode;

        try {
            using var document = JsonDocument.Parse(body);
            if (GetString(document.RootElement, "error") is { } error) {
                var description = GetString(document.RootElement, "error_description");

                return new AccessTokenRequestException(
                    message: $"OAuth error '{error}'{(description is null ? string.Empty : $": {description}")}",
                    statusCode: statusCode,
                    error: error,
                    errorDescription: description
                );
            }
        }
        catch (JsonException) {
            // Proxies and gateways often reply with HTML or plain text;
            // that is not an OAuth error.
        }

        return new AccessTokenRequestException(
            message: $"Token request failed with HTTP {statusCode}.",
            statusCode: statusCode,
            errorDescription: Truncate(body)
        );
    }

    private AccessToken ReadToken(JsonElement root, int statusCode) {
        if (GetString(root, "access_token") is not { Length: > 0 } accessToken) {
            throw new AccessTokenRequestException(
                message: "Token response is missing 'access_token'.",
                statusCode: statusCode
            );
        }

        DateTimeOffset? expiresAt = GetSeconds(root, "expires_in") is { } seconds
            ? TimeProvider.GetUtcNow().AddSeconds(seconds)
            : null;

        Dictionary<string, JsonElement>? extra = null;
        foreach (var property in root.EnumerateObject()) {
            if (KnownFields.Contains(property.Name)) { continue; }

            // Clone, because the JsonDocument is disposed before the
            // caller uses the token.
            extra ??= new Dictionary<string, JsonElement>(StringComparer.Ordinal);

            extra[property.Name] = property.Value.Clone();
        }

        return new AccessToken(
            Value: accessToken,
            TokenType: GetString(root, "token_type") ?? "Bearer",
            ExpiresAt: expiresAt,
            Scope: GetString(root, "scope"),
            RefreshToken: GetString(root, "refresh_token"),
            Extra: extra
        );
    }

    private static string? GetString(JsonElement root, string name) {
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(name, out var property) &&
            property.ValueKind == JsonValueKind.String) { return property.GetString(); }

        return null;
    }

    private static double? GetSeconds(JsonElement root, string name) {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(name, out var property)) {
            return null;
        }

        // The RFC defines a number, but some vendors return a string.
        return property.ValueKind switch {
            JsonValueKind.Number when property.TryGetDouble(out var number) => number,
            JsonValueKind.String when double.TryParse(
                property.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var parsed
            ) => parsed,
            _ => null
        };
    }
}

