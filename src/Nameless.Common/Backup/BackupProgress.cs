namespace Nameless.Backup;

/// <summary>
///     Progress of a backup or restore.
/// </summary>
/// <param name="Stage">
///     Current stage.
/// </param>
/// <param name="RoutineKey">
///     Key of the routine being run, when the stage concerns a routine.
/// </param>
public sealed record BackupProgress(BackupStage Stage, string? RoutineKey = null);
