namespace Nameless.ObjectModel;

/// <summary>
///     Represents an empty result object.
/// </summary>
public sealed class EmptyResult : Result<Nothing> {
    private EmptyResult(Nothing? value, Error[] errors) : base (value, errors) { }

    /// <summary>
    ///     Converts a <see cref="Nothing"/> instance into an
    ///     <see cref="EmptyResult"/> instance.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="Nothing"/> value.
    /// </param>
    public static implicit operator EmptyResult(Nothing value) {
        return new EmptyResult(value, errors: []);
    }

    /// <summary>
    ///     Converts an <see cref="Error"/> instance into an
    ///     <see cref="EmptyResult"/> instance.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="Error"/> value.
    /// </param>
    public static implicit operator EmptyResult(Error error) {
        return new EmptyResult(value: null, errors: [error]);
    }

    /// <summary>
    ///     Converts a collection of <see cref="Error"/> into an
    ///     <see cref="EmptyResult"/> instance.
    /// </summary>
    /// <param name="errors">
    ///     The collection of <see cref="Error"/>.
    /// </param>
    public static implicit operator EmptyResult(Error[] errors) {
        return new EmptyResult(value: null, errors: errors);
    }
}
