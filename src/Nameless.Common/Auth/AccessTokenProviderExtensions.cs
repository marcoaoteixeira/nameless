using Nameless.ObjectModel;

namespace Nameless.Auth;

/// <summary>
///     <see cref="IAccessTokenProvider{TRequest}"/> extension methods.
/// </summary>
public static class AccessTokenProviderExtensions {
    /// <typeparam name="TRequest">
    ///     Type of the request.
    /// </typeparam>
    /// <param name="self">
    ///     The current <see cref="IAccessTokenProvider{TRequest}"/> instance.
    /// </param>
    extension<TRequest>(IAccessTokenProvider<TRequest> self) where TRequest : notnull {
        /// <summary>
        ///     Gets a token for the given request.
        /// </summary>
        /// <param name="request">
        ///     Request parameters.
        /// </param>
        /// <param name="timeout">
        ///     Timeout for the HTTP request.
        /// </param>
        /// <returns>
        ///     A usable token; the implementation decides whether it comes from a
        ///     cache or a new request.
        /// </returns>
        public AccessTokenResult GetToken(TRequest request, TimeSpan? timeout = null) {
            timeout ??= TimeSpan.FromSeconds(15);

            Throws.When.LowerThan(timeout.Value, paramName: nameof(timeout), compare: TimeSpan.Zero);

            using var cts = new CancellationTokenSource(timeout.Value);

            try {
                return self.GetTokenAsync(request, cts.Token)
                           .GetAwaiter()
                           .GetResult();
            }
            catch (Exception ex) { return Error.Failure(ex.Message, ex: ex); }
        }
    }
}