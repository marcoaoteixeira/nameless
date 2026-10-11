using System.Text.Json;

namespace Nameless.Auth;

/// <summary>
///     A token obtained from a source, independent of the originating
///     protocol.
/// </summary>
/// <param name="Value">
///     Token value (e.g. OAuth <c>access_token</c>, JWT).
/// </param>
/// <param name="TokenType">
///     Scheme for the <c>Authorization</c> header, e.g. <c>Bearer</c>.
/// </param>
/// <param name="ExpiresAt">
///     Absolute expiration; <see langword="null"/> if the token does
///     not expire or the expiration is unknown.
/// </param>
/// <param name="Scope">
///     Granted scopes, if the source returns them.
/// </param>
/// <param name="RefreshToken">
///     Refresh token, if any.
/// </param>
/// <param name="Extra">
///     Additional response fields (e.g. <c>id_token</c>).
/// </param>
public sealed record AccessToken(
    string Value,
    string TokenType = "Bearer",
    DateTimeOffset? ExpiresAt = null,
    string? Scope = null,
    string? RefreshToken = null,
    IReadOnlyDictionary<string, JsonElement>? Extra = null
) {
    /// <summary>
    ///     Indicates whether the token expires within the given margin
    ///     (or has already expired).
    /// </summary>
    /// <param name="timeProvider">
    ///     Source of the current instant.
    /// </param>
    /// <param name="skew">
    ///     Safety margin before the actual expiration.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the token should no longer be used.
    /// </returns>
    public bool IsExpired(TimeProvider timeProvider, TimeSpan skew) {
        return ExpiresAt is { } expiresAt && expiresAt - skew <= timeProvider.GetUtcNow();
    }

    /// <summary>
    ///     Log-safe representation: never includes the token value.
    /// </summary>
    /// <returns>
    ///     The token type and expiration.
    /// </returns>
    public override string ToString() {
        return $"AccessToken {{ TokenType = {TokenType}, ExpiresAt = {ExpiresAt:O} }}";
    }
}
