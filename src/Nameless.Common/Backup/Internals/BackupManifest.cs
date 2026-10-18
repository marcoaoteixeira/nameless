using Nameless.ObjectModel;

namespace Nameless.Backup.Internals;

/// <summary>
///     Describes the content of a backup archive.
/// </summary>
/// <param name="FormatVersion">
///     Archive format version.
/// </param>
/// <param name="CreatedAt">
///     UTC timestamp of the backup.
/// </param>
/// <param name="Routines">
///     The routines stored in the archive.
/// </param>
internal sealed record BackupManifest(
    int FormatVersion,
    DateTimeOffset CreatedAt,
    IReadOnlyList<BackupManifestRoutine> Routines
) {
    public const int CurrentFormatVersion = 1;

    public const string FileName = "manifest.json";

    /// <summary>
    ///     Name of the single root folder of the archive (the payload).
    /// </summary>
    public const string PayloadDirectoryName = "backup";
}

/// <summary>
///     A result for a backup manifest.
/// </summary>
internal sealed class BackupManifestResult : Result<BackupManifest> {
    private BackupManifestResult(BackupManifest? value, Error[] errors)
        : base(value, errors) { }

    public static implicit operator BackupManifestResult(BackupManifest value) {
        return new BackupManifestResult(value, errors: []);
    }

    public static implicit operator BackupManifestResult(Error error) {
        return new BackupManifestResult(value: null, errors: [error]);
    }

    public static implicit operator BackupManifestResult(Error[] errors) {
        return new BackupManifestResult(value: null, errors: errors);
    }
}

/// <summary>
///     A routine stored in the archive.
/// </summary>
/// <param name="Key">
///     The routine key (its folder inside the payload).
/// </param>
/// <param name="Order">
///     The routine order at backup time (informative).
/// </param>
/// <param name="Files">
///     The routine files.
/// </param>
internal sealed record BackupManifestRoutine(
    string Key,
    int Order,
    IReadOnlyList<BackupManifestFile> Files
);

/// <summary>
///     A result for a backup manifest routine.
/// </summary>
internal sealed class BackupManifestRoutineResult : Result<BackupManifestRoutine> {
    private BackupManifestRoutineResult(BackupManifestRoutine? value, Error[] errors)
        : base(value, errors) { }

    public static implicit operator BackupManifestRoutineResult(BackupManifestRoutine value) {
        return new BackupManifestRoutineResult(value, errors: []);
    }

    public static implicit operator BackupManifestRoutineResult(Error error) {
        return new BackupManifestRoutineResult(value: null, errors: [error]);
    }
}

/// <summary>
///     A file written by a routine.
/// </summary>
/// <param name="Path">
///     Path relative to the routine folder, <c>/</c>-separated.
/// </param>
/// <param name="Length">
///     File length in bytes.
/// </param>
/// <param name="Sha256">
///     Lowercase hex SHA-256 of the file content.
/// </param>
internal sealed record BackupManifestFile(string Path, long Length, string Sha256);
