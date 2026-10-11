namespace Nameless.Auth.OAuth;

/// <summary>
///     The <c>authorization_code</c> grant (RFC 6749 §4.1.3), with
///     optional PKCE (RFC 7636).
/// </summary>
/// <param name="Code">
///     Code received in the redirect.
/// </param>
/// <param name="RedirectUri">
///     Must match the one sent in the authorization request, if that
///     request included it.
/// </param>
/// <param name="CodeVerifier">
///     PKCE verifier; required if the authorization request sent a
///     <c>code_challenge</c>.
/// </param>
public sealed record AuthorizationCodeRequest(string Code, string? RedirectUri = null, string? CodeVerifier = null) : OAuthTokenRequest {
    /// <inheritdoc />
    public override string GrantType => "authorization_code";

    /// <inheritdoc />
    internal override void WriteParameters(IDictionary<string, string> form) {
        Include(form, "code", Code);
        Include(form, "redirect_uri", RedirectUri);
        Include(form, "code_verifier", CodeVerifier);
    }
}