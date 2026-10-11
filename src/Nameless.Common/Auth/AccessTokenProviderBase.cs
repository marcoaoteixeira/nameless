namespace Nameless.Auth;

/// <summary>
///     Base for HTTP providers: sends the request, handles failures, and
///     delegates building the request and reading the response to derived
///     classes.
/// </summary>
/// <typeparam name="TRequest">
///     Parameters of the token request.
/// </typeparam>
/// <remarks>
///     A client is requested from the factory on every call instead of being
///     stored, so that the <see cref="IHttpClientFactory"/> handler rotation
///     (DNS, certificates) keeps working in a provider registered as a
///     singleton.
/// </remarks>
public abstract class AccessTokenProviderBase<TRequest> : IAccessTokenProvider<TRequest> where TRequest : notnull {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _httpClientName;
    private const int TRUNCATE_BODY_LENGTH = 500;

    /// <summary>
    ///     Source of the current instant.
    /// </summary>
    protected TimeProvider TimeProvider { get; }

    /// <summary>
    ///     Initializes the base provider.
    /// </summary>
    /// <param name="httpClientFactory">
    ///     Factory used to create one client per request.
    /// </param>
    /// <param name="httpClientName">
    ///     Name of the client configured in the factory.
    /// </param>
    /// <param name="timeProvider">
    ///     Source of the current instant, used to compute expirations.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="httpClientFactory"/> or
    ///     <paramref name="timeProvider"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="httpClientName"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    protected AccessTokenProviderBase(IHttpClientFactory httpClientFactory, string httpClientName, TimeProvider timeProvider) {
        _httpClientFactory = Throws.When.Null(httpClientFactory);
        _httpClientName = Throws.When.Null(httpClientName);
        TimeProvider = Throws.When.Null(timeProvider);
    }
    
    /// <summary>
    ///     Builds the HTTP request that returns the token.
    /// </summary>
    /// <param name="request">
    ///     Parameters of the token request.
    /// </param>
    /// <returns>
    ///     A request ready to send; the base class disposes it after sending.
    /// </returns>
    protected abstract HttpRequestMessage CreateRequest(TRequest request);

    /// <summary>
    ///     Reads a successful (2xx) response and returns the token.
    /// </summary>
    /// <param name="response">
    ///     Successful HTTP response.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    /// <returns>
    ///     The token described by the response.
    /// </returns>
    /// <exception cref="AccessTokenRequestException">
    ///     The response does not contain a usable token.
    /// </exception>
    protected abstract ValueTask<AccessToken> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken);

    /// <summary>
    ///     Converts an error (non-2xx) response into an exception.
    /// </summary>
    /// <param name="response">
    ///     Error HTTP response.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancellation token.
    /// </param>
    /// <returns>
    ///     The exception to throw.
    /// </returns>
    /// <remarks>
    ///     Override to interpret the source's own error format.
    /// </remarks>
    protected virtual async ValueTask<AccessTokenRequestException> CreateExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken) {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).SkipContextSync();

        return new AccessTokenRequestException(
            message: $"Token request failed with HTTP {(int)response.StatusCode}.",
            statusCode: (int)response.StatusCode,
            errorDescription: Truncate(body)
        );
    }

    /// <inheritdoc />
    /// <exception cref="AccessTokenRequestException">
    ///     The source rejected the request, is unreachable, or replied with
    ///     an invalid response.
    /// </exception>
    public async ValueTask<AccessToken> GetTokenAsync(TRequest request, CancellationToken cancellationToken = default) {
        Throws.When.Null(request);

        var client = _httpClientFactory.CreateClient(_httpClientName);
        
        using var httpRequest = CreateRequest(request);

        HttpResponseMessage response;
        try { response = await client.SendAsync(httpRequest, cancellationToken).SkipContextSync(); }
        catch (HttpRequestException ex) {
            // Callers only need to handle one exception type, whether the
            // failure came from the network or the source.
            throw new AccessTokenRequestException("Token endpoint unreachable.", inner: ex);
        }

        using (response) {
            if (!response.IsSuccessStatusCode) {
                throw await CreateExceptionAsync(response, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
            }

            return await ParseResponseAsync(response, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    /// <summary>
    ///     Limits the body included in an exception, so whole error pages
    ///     do not end up in logs.
    /// </summary>
    /// <param name="body">
    ///     Original text.
    /// </param>
    /// <returns>
    ///     The first <see cref="TRUNCATE_BODY_LENGTH"/> characters.
    /// </returns>
    protected static string Truncate(string body) {
        return body.Length > TRUNCATE_BODY_LENGTH ? body[..TRUNCATE_BODY_LENGTH] : body;
    }
}
