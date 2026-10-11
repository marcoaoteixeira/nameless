namespace Nameless.Auth;

/// <summary>
///     A source able to provide authorization tokens for a given kind of
///     request.
/// </summary>
/// <typeparam name="TRequest">
///     Parameters of the token request. Must have value equality
///     (e.g. a <c>record</c>), because the cache uses it as a key.
/// </typeparam>
/// <remarks>
///     The type parameter is contravariant: a provider of
///     <c>OAuthTokenRequest</c> can be used wherever a provider of any
///     derived request is expected.
/// </remarks>
public interface IAccessTokenProvider<in TRequest> where TRequest : notnull {
    /// <summary>
    ///     Gets a token for the given request.
    /// </summary>
    /// <param name="request">
    ///     Request parameters.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    /// <returns>
    ///     A usable token; the implementation decides whether it comes from a
    ///     cache or a new request.
    /// </returns>
    ValueTask<AccessToken> GetTokenAsync(TRequest request, CancellationToken cancellationToken);

    /// <summary>
    ///     Discards the stored token for the request (e.g. after a 401 from
    ///     the protected resource).
    /// </summary>
    /// <param name="request">
    ///     Request whose token must be discarded.
    /// </param>
    /// <remarks>
    ///     Does nothing by default, so stateless providers do not need to
    ///     implement it.
    /// </remarks>
    void Invalidate(TRequest request) { }
}
