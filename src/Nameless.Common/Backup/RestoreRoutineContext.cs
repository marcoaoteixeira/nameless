namespace Nameless.Backup;

/// <summary>
///     Context handed to <see cref="IBackupRoutine.RestoreAsync"/>.
/// </summary>
/// <param name="WorkingDirectoryPath">
///     Existing directory holding the files the routine wrote on backup.
/// </param>
/// <param name="BackupCreatedAt">
///     UTC timestamp of the backup being restored.
/// </param>
/// <param name="IsRollback">
///     <see langword="true"/> when the routine is being restored from the
///     safety snapshot because a restore failed.
/// </param>
public sealed record RestoreRoutineContext(
    string WorkingDirectoryPath,
    DateTimeOffset BackupCreatedAt,
    bool IsRollback
);
