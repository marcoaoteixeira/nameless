using Nameless.ObjectModel;

namespace Nameless.Auth;

/// <summary>
///     <see cref="AccessToken"/> result.
/// </summary>
public class AccessTokenResult : Result<AccessToken> {
    private AccessTokenResult(AccessToken? value, Error[] errors)
        : base(value, errors) { }

    /// <summary>
    ///     Implicit converts <see cref="AccessToken"/> into
    ///     <see cref="AccessTokenResult"/>.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="AccessToken"/> instance.
    /// </param>
    /// <returns>
    ///     An <see cref="AccessTokenResult"/> instance.
    /// </returns>
    public static implicit operator AccessTokenResult(AccessToken value) {
        return new AccessTokenResult(value, errors: []);
    }

    /// <summary>
    ///     Implicit converts <see cref="Error"/> into
    ///     <see cref="AccessTokenResult"/>.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="Error"/> instance.
    /// </param>
    /// <returns>
    ///     An <see cref="AccessTokenResult"/> instance.
    /// </returns>
    public static implicit operator AccessTokenResult(Error error) {
        return new AccessTokenResult(value: null, errors: [error]);
    }
}