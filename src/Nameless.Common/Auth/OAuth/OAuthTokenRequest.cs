namespace Nameless.Auth.OAuth;

/// <summary>
///     Parameters of a request to the Token Endpoint. Each derived type
///     represents a grant (RFC 6749 §4); use <see cref="CustomGrantRequest"/>
///     for extension grants.
/// </summary>
/// <remarks>
///     The hierarchy is closed on purpose (<see langword="private protected"/>
///     constructor): every grant goes through <see cref="WriteParameters"/>,
///     and extensibility lives in <see cref="CustomGrantRequest"/>. The types
///     have value equality so they can be used as cache keys.
/// </remarks>
public abstract record OAuthTokenRequest {
    /// <summary>
    ///     Value of the <c>grant_type</c> parameter.
    /// </summary>
    public abstract string GrantType { get; }

    private protected OAuthTokenRequest() { }

    /// <summary>
    ///     Adds the parameters specific to this grant to the request form.
    /// </summary>
    /// <param name="form">
    ///     Form to fill; <c>grant_type</c> is already present.
    /// </param>
    internal abstract void WriteParameters(IDictionary<string, string> form);

    /// <summary>Log-safe representation: never includes codes, secrets, or refresh tokens.</summary>
    /// <returns>The type name and grant.</returns>
    public sealed override string ToString() {
        return $"{GetType().Name} {{ GrantType = {GrantType} }}";
    }

    /// <summary>
    ///     Adds the parameter only if it has a value, so empty fields are
    ///     not sent to the server.
    /// </summary>
    /// <param name="form">
    ///     Form to fill.
    /// </param>
    /// <param name="name">
    ///     Parameter name.
    /// </param>
    /// <param name="value">
    ///     Value; ignored if null or empty.
    /// </param>
    private protected static void Include(IDictionary<string, string> form, string name, string? value) {
        if (!string.IsNullOrEmpty(value)) {
            form[name] = value;
        }
    }
}