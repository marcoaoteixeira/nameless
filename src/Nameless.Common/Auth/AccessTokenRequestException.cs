namespace Nameless.Auth;

/// <summary>Failure to obtain a token from a source.</summary>
public class AccessTokenRequestException : Exception {
    /// <summary>
    ///     HTTP status code of the response, or <see langword="null"/>
    ///     if there was no response.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    ///     Error code from the source (RFC 6749 §5.2 in OAuth).
    /// </summary>
    public string? Error { get; }

    /// <summary>
    ///     Error detail returned by the source.
    /// </summary>
    public string? ErrorDescription { get; }

    /// <summary>
    ///     Initializes a new instance of
    ///     <see cref="AccessTokenRequestException"/> class.
    /// </summary>
    /// <param name="message">
    ///     Description of the failure.
    /// </param>
    /// <param name="statusCode">
    ///     HTTP status code of the response, if there was one.
    /// </param>
    /// <param name="error">
    ///     Error code returned by the source (e.g. <c>invalid_grant</c> in OAuth).
    /// </param>
    /// <param name="errorDescription">
    ///     Detail returned by the source.
    /// </param>
    /// <param name="inner">
    ///     Original exception, if any.
    /// </param>
    public AccessTokenRequestException(string message, int? statusCode = null, string? error = null, string? errorDescription = null, Exception? inner = null) : base(message, inner) {
        StatusCode = statusCode;
        Error = error;
        ErrorDescription = errorDescription;
    }
}