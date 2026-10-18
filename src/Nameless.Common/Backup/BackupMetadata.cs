using Nameless.Compression;
using Nameless.ObjectModel;

namespace Nameless.Backup;

/// <summary>
///     Outcome of a successful backup.
/// </summary>
/// <param name="FilePath">
///     Full path of the archive.
/// </param>
/// <param name="Checksum">
///     Archive checksum. Keep it where the archive's holder can't alter it
///     and pass it to <see cref="RestoreRequest.Checksum"/> to detect
///     tampering.
/// </param>
/// <param name="Algorithm">
///     How <paramref name="Checksum"/> was computed.
/// </param>
/// <param name="CreatedAt">
///     UTC timestamp of the backup.
/// </param>
/// <param name="RoutineKeys">
///     Keys of the routines stored in the archive, in execution order.
/// </param>
public sealed record BackupMetadata(
    string FilePath,
    string Checksum,
    ChecksumAlgorithm Algorithm,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> RoutineKeys
);

/// <summary>
///     A result for a restore info.
/// </summary>
public sealed class BackupResult : Result<BackupMetadata> {
    private BackupResult(BackupMetadata? value, Error[] errors)
        : base(value, errors) { }

    /// <summary>
    ///     Converts a <see cref="BackupMetadata"/> instance into a
    ///     <see cref="BackupResult"/> instance.
    /// </summary>
    /// <param name="value">
    ///     The <see cref="BackupMetadata"/> instance.
    /// </param>
    public static implicit operator BackupResult(BackupMetadata value) {
        return new BackupResult(value, errors: []);
    }

    /// <summary>
    ///     Converts an <see cref="Error"/> instance into a
    ///     <see cref="BackupResult"/> instance.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="Error"/> instance.
    /// </param>
    public static implicit operator BackupResult(Error error) {
        return new BackupResult(value: null, errors: [error]);
    }

    /// <summary>
    ///     Converts a collection of <see cref="Error"/> instances into a
    ///     <see cref="BackupResult"/> instance.
    /// </summary>
    /// <param name="errors">
    ///     The collection of <see cref="Error"/> instances.
    /// </param>
    public static implicit operator BackupResult(Error[] errors) {
        return new BackupResult(value: null, errors: errors);
    }
}