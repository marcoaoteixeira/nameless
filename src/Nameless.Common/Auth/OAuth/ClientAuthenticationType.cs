namespace Nameless.Auth.OAuth;

/// <summary>
///     How the client authenticates at the Token Endpoint (RFC 6749 §2.3).
/// </summary>
public enum ClientAuthenticationType {
    /// <summary>
    ///     <c>Authorization: Basic</c> header with the encoded id and secret.
    /// </summary>
    ClientSecretBasic,

    /// <summary>
    ///     <c>client_id</c> and <c>client_secret</c> in the request body.
    /// </summary>
    ClientSecretPost,

    /// <summary>
    ///     Public client: sends only <c>client_id</c> (e.g. Authorization
    ///     Code with PKCE).
    /// </summary>
    None
}