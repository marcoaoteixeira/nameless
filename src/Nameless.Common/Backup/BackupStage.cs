namespace Nameless.Backup;

/// <summary>
///     Stages reported through <see cref="BackupProgress"/>.
/// </summary>
public enum BackupStage {
    /// <summary>
    ///     Validating the request and preparing the staging directory.
    /// </summary>
    Preparing,

    /// <summary>
    ///     Running a routine's backup.
    /// </summary>
    BackingUp,

    /// <summary>
    ///     Writing the archive.
    /// </summary>
    Compressing,

    /// <summary>
    ///     Extracting the archive.
    /// </summary>
    Extracting,

    /// <summary>
    ///     Validating the archive content against its manifest.
    /// </summary>
    Validating,

    /// <summary>
    ///     Backing up the current state of a routine before restoring it.
    /// </summary>
    CreatingSafetySnapshot,

    /// <summary>
    ///     Running a routine's restore.
    /// </summary>
    Restoring,

    /// <summary>
    ///     Putting a routine's previous state back after a failed restore.
    /// </summary>
    RollingBack,

    /// <summary>
    ///     Deleting temporary files.
    /// </summary>
    CleaningUp,

    /// <summary>
    ///     The operation succeeded.
    /// </summary>
    Completed
}
