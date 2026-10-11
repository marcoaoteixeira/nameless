using Nameless.GitHub.ObjectModel;
using Nameless.ObjectModel;

namespace Nameless.GitHub;

/// <summary>
///     Represents a response to a request for the latest release
/// </summary>
public class GetLastestReleaseResult : Result<Release> {
    private GetLastestReleaseResult(Release? value, Error[] errors)
        : base (value, errors) { }

    /// <summary>
    ///     Converts the <see cref="Release"/> object instance into a
    ///     <see cref="GetLastestReleaseResult"/> object.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="Release"/> object instance.
    /// </param>
    public static implicit operator GetLastestReleaseResult(Release value) {
        return new GetLastestReleaseResult(value, errors: []);
    }

    /// <summary>
    ///     Converts the <see cref="Error"/> object instance into a
    ///     <see cref="GetLastestReleaseResult"/> object.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="Error"/> object instance.
    /// </param>
    public static implicit operator GetLastestReleaseResult(Error error) {
        return new GetLastestReleaseResult(value: null, errors: [error]);
    }
}