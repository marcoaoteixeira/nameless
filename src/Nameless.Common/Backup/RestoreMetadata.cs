using Nameless.ObjectModel;

namespace Nameless.Backup;

/// <summary>
///     Outcome of a successful restore.
/// </summary>
/// <param name="BackupCreatedAt">
///     UTC timestamp of the restored backup.
/// </param>
/// <param name="RestoredRoutineKeys">
///     Keys of the restored routines, in execution order.
/// </param>
/// <param name="SkippedRoutineKeys">
///     Keys of registered routines absent from the archive (left
///     untouched).
/// </param>
public sealed record RestoreMetadata(
    DateTimeOffset BackupCreatedAt,
    IReadOnlyList<string> RestoredRoutineKeys,
    IReadOnlyList<string> SkippedRoutineKeys
);

/// <summary>
///     A result for a restore info.
/// </summary>
public sealed class RestoreResult : Result<RestoreMetadata> {
    private RestoreResult(RestoreMetadata? value, Error[] errors)
        : base(value, errors) { }

    /// <summary>
    ///     Converts a <see cref="RestoreMetadata"/> instance into a
    ///     <see cref="RestoreResult"/> instance.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="RestoreMetadata"/> instance.
    /// </param>
    public static implicit operator RestoreResult(RestoreMetadata value) {
        return new RestoreResult(value, errors: []);
    }

    /// <summary>
    ///     Converts an <see cref="Error"/> instance into a
    ///     <see cref="RestoreResult"/> instance.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="Error"/> instance.
    /// </param>
    public static implicit operator RestoreResult(Error error) {
        return new RestoreResult(value: null, errors: [error]);
    }

    /// <summary>
    ///     Converts a collection of <see cref="Error"/> instances into a
    ///     <see cref="RestoreResult"/> instance.
    /// </summary>
    /// <param name="errors">
    ///     The collection of <see cref="Error"/> instances.
    /// </param>
    public static implicit operator RestoreResult(Error[] errors) {
        return new RestoreResult(value: null, errors: errors);
    }
}