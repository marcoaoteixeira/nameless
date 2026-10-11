using Nameless.GitHub.ObjectModel;
using Nameless.ObjectModel;

namespace Nameless.GitHub;

/// <summary>
///     Represents a response to a request for the release assets
/// </summary>
public class GetReleaseAssetsResult : Result<ReleaseAsset[]> {
    private GetReleaseAssetsResult(ReleaseAsset[] value, Error[] errors)
        : base(value, errors) { }

    /// <summary>
    ///     Converts the <see cref="ReleaseAsset"/> array instance into a
    ///     <see cref="GetReleaseAssetsResult"/> object.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="ReleaseAsset"/> array instance.
    /// </param>
    public static implicit operator GetReleaseAssetsResult(ReleaseAsset[] value) {
        return new GetReleaseAssetsResult(value, errors: []);
    }

    /// <summary>
    ///     Converts the <see cref="Error"/> object instance into a
    ///     <see cref="GetReleaseAssetsResult"/> object.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="Error"/> object instance.
    /// </param>
    public static implicit operator GetReleaseAssetsResult(Error error) {
        return new GetReleaseAssetsResult(value: [], errors: [error]);
    }
}