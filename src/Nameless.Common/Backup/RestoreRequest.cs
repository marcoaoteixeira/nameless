namespace Nameless.Backup;

/// <summary>
///     Parameters of a restore.
/// </summary>
public sealed record RestoreRequest {
    /// <summary>
    ///     Gets the path of an archive created by
    ///     <see cref="IBackupService.BackupAsync"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     if the value is <see langword="null"/>, empty or white space.
    /// </exception>
    public required string ArchiveFilePath {
        get;
        init => field = Throws.When.NullOrWhiteSpace(value, paramName: nameof(ArchiveFilePath));
    }

    /// <summary>
    ///     Gets the expected archive checksum
    ///     (<see cref="BackupMetadata.Checksum"/>);
    ///     <see langword="null"/> skips the archive verification. The
    ///     per-file hashes of the manifest are always verified.
    /// </summary>
    public string? Checksum { get; init; }
}
