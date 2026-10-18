using Microsoft.Extensions.Logging;

namespace Nameless.Backup;

/// <summary>
///     High-performance log messages (source generated). File contents and
///     checksums are never logged.
/// </summary>
internal static partial class Log {
    // ---- Backup (1xx) -----------------------------------------------------

    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Backup of {RoutineCount} routines into '{Destination}' started.")]
    public static partial void BackupStarted(ILogger logger, int routineCount, string destination);

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Backup into '{Destination}' completed in {ElapsedMilliseconds:0} ms.")]
    public static partial void BackupCompleted(ILogger logger, string destination, double elapsedMilliseconds);

    [LoggerMessage(EventId = 102, Level = LogLevel.Warning, Message = "Backup into '{Destination}' failed: {Reason}")]
    public static partial void BackupFailed(ILogger logger, string destination, string reason);

    // ---- Restore (2xx) ----------------------------------------------------

    [LoggerMessage(EventId = 200, Level = LogLevel.Information, Message = "Restore from '{Archive}' started.")]
    public static partial void RestoreStarted(ILogger logger, string archive);

    [LoggerMessage(EventId = 201, Level = LogLevel.Information, Message = "Restore from '{Archive}' completed in {ElapsedMilliseconds:0} ms: {RestoredCount} routines restored, {SkippedCount} skipped.")]
    public static partial void RestoreCompleted(ILogger logger, string archive, double elapsedMilliseconds, int restoredCount, int skippedCount);

    [LoggerMessage(EventId = 202, Level = LogLevel.Warning, Message = "Restore from '{Archive}' failed: {Reason}")]
    public static partial void RestoreFailed(ILogger logger, string archive, string reason);

    [LoggerMessage(EventId = 203, Level = LogLevel.Warning, Message = "Restore interrupted at routine '{RoutineKey}'; rolling back {RoutineCount} routines.")]
    public static partial void RollingBack(ILogger logger, string routineKey, int routineCount);

    [LoggerMessage(EventId = 204, Level = LogLevel.Critical, Message = "Rollback failed. The safety snapshot was kept at '{SnapshotPath}' for manual recovery.")]
    public static partial void RollbackFailed(ILogger logger, string snapshotPath);

    // ---- Routines & housekeeping (3xx) ------------------------------------

    [LoggerMessage(EventId = 300, Level = LogLevel.Warning, Message = "Backup routine '{RoutineKey}' failed its {Operation}: {Reason}")]
    public static partial void RoutineFailed(ILogger logger, string routineKey, string operation, string reason);

    [LoggerMessage(EventId = 301, Level = LogLevel.Error, Message = "Backup routine '{RoutineKey}' threw during its {Operation}.")]
    public static partial void RoutineThrew(ILogger logger, Exception exception, string routineKey, string operation);

    [LoggerMessage(EventId = 302, Level = LogLevel.Warning, Message = "Could not delete the staging directory '{Path}'.")]
    public static partial void CleanupFailed(ILogger logger, Exception exception, string path);

    [LoggerMessage(EventId = 303, Level = LogLevel.Error, Message = "Could not move the safety snapshot out of '{Path}'; the staging directory is kept instead.")]
    public static partial void SnapshotMoveFailed(ILogger logger, Exception exception, string path);

    [LoggerMessage(EventId = 304, Level = LogLevel.Warning, Message = "The progress receiver threw while reporting {Stage}; ignored during rollback.")]
    public static partial void ProgressReportFailed(ILogger logger, Exception exception, BackupStage stage);
}
