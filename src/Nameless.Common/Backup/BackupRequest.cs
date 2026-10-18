namespace Nameless.Backup;

/// <summary>
///     Parameters of a backup.
/// </summary>
public sealed record BackupRequest {
    /// <summary>
    ///     Gets the archive path. Missing parent directories are created.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     if the value is <see langword="null"/>, empty or white space.
    /// </exception>
    public required string DestinationFilePath {
        get;
        init => field = Throws.When.NullOrWhiteSpace(value, paramName: nameof(DestinationFilePath));
    }

    /// <summary>
    ///     Gets whether an existing archive at
    ///     <see cref="DestinationFilePath"/> may be replaced. Default
    ///     <see langword="false"/>.
    /// </summary>
    public bool Overwrite { get; init; }
}
