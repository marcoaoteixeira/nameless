namespace Nameless.Auth.OAuth;

/// <summary>
///     An extension grant, or one not covered by the other types (e.g. JWT
///     bearer, token exchange, device code).
/// </summary>
public sealed record CustomGrantRequest : OAuthTokenRequest {
    /// <inheritdoc />
    public override string GrantType { get; }

    /// <summary>Additional parameters sent in the request.</summary>
    public IReadOnlyDictionary<string, string> Parameters { get; }

    /// <summary>
    ///     Creates the request.
    /// </summary>
    /// <param name="grantType">
    ///     Value of <c>grant_type</c>, e.g.
    ///     <c>urn:ietf:params:oauth:grant-type:jwt-bearer</c>.
    /// </param>
    /// <param name="parameters">
    ///     Additional request parameters.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="grantType"/> is null, empty, or whitespace.
    /// </exception>
    public CustomGrantRequest(string grantType, IReadOnlyDictionary<string, string>? parameters = null) {
        GrantType = Throws.When.NullOrWhiteSpace(grantType);

        // Defensive copy: this object is a cache key, and changing the
        // original dictionary after insertion would change the hash and
        // lose the entry.
        Parameters = parameters is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(parameters, StringComparer.Ordinal);
    }
    
    /// <inheritdoc />
    internal override void WriteParameters(IDictionary<string, string> form) {
        foreach (var (name, value) in Parameters) {
            Include(form, name, value);
        }
    }

    /// <summary>
    ///     Compares the grant and the parameters, ignoring the order of
    ///     the latter.
    /// </summary>
    /// <param name="other">
    ///     Request to compare with.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if they are equivalent.</returns>
    public bool Equals(CustomGrantRequest? other) {
        if (other is null) {
            return false;
        }

        // Dictionaries do not have value equality, so the record-generated
        // Equals would not work.
        return GrantType == other.GrantType &&
               Parameters.Count == other.Parameters.Count &&
               Parameters.All(param => other.Parameters.TryGetValue(param.Key, out var value) &&
                                       value == param.Value);
    }

    /// <summary>
    ///     Hash consistent with <see cref="Equals(CustomGrantRequest?)"/>.
    /// </summary>
    /// <returns>
    ///     A hash that does not depend on the order of the parameters.
    /// </returns>
    public override int GetHashCode() {
        // A sum instead of a chained combination, so the parameter order does
        // not affect the hash.
        var parametersHash = 0;

        foreach (var (name, value) in Parameters) {
            parametersHash += HashCode.Combine(name, value);
        }

        return HashCode.Combine(GrantType, parametersHash);
    }
}