namespace Nameless.Auth.OAuth;

/// <summary>
///     The <c>client_credentials</c> grant (RFC 6749 §4.4).
/// </summary>
/// <param name="Scope">
///     Requested scopes, separated by spaces.
/// </param>
/// <param name="Audience">
///     Intended recipient of the token (<c>audience</c>); used by several
///     vendors, not part of RFC 6749.
/// </param>
public sealed record ClientCredentialsRequest(string? Scope = null, string? Audience = null) : OAuthTokenRequest {
    /// <inheritdoc />
    public override string GrantType => "client_credentials";

    /// <inheritdoc />
    internal override void WriteParameters(IDictionary<string, string> form) {
        Include(form, "scope", Scope);
        Include(form, "audience", Audience);
    }
}