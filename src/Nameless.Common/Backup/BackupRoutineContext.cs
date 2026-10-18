namespace Nameless.Backup;

/// <summary>
///     Context handed to <see cref="IBackupRoutine.BackupAsync"/>.
/// </summary>
/// <param name="WorkingDirectoryPath">
///     Existing, empty directory where the routine writes its files.
/// </param>
/// <param name="CreatedAt">
///     UTC timestamp of the backup.
/// </param>
public sealed record BackupRoutineContext(string WorkingDirectoryPath, DateTimeOffset CreatedAt);
