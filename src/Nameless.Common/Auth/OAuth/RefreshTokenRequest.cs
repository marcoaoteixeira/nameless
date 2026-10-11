namespace Nameless.Auth.OAuth;

/// <summary>The <c>refresh_token</c> grant (RFC 6749 §6).</summary>
/// <param name="RefreshToken">Previously issued refresh token.</param>
/// <param name="Scope">Subset of the original scopes; omit to keep all of them.</param>
public sealed record RefreshTokenRequest(string RefreshToken, string? Scope = null) : OAuthTokenRequest {
    /// <inheritdoc />
    public override string GrantType => "refresh_token";

    /// <inheritdoc />
    internal override void WriteParameters(IDictionary<string, string> form) {
        Include(form, "refresh_token", RefreshToken);
        Include(form, "scope", Scope);
    }
}