using Nameless.ObjectModel;

namespace Nameless.Backup;

/// <summary>
///     Backs up and restores one piece of application state (a database,
///     a folder, settings, ...). <see cref="IBackupService"/> orchestrates
///     every registered routine.
/// </summary>
/// <remarks>
///     <para>
///         A routine must only read and write inside the working directory
///         handed to it by the context; that directory becomes the
///         routine's folder inside the archive. Symbolic links and junctions
///         left there fail the backup.
///     </para>
///     <para>
///         Routines run sequentially, never concurrently, so they may share
///         resources (e.g. a scoped <c>DbContext</c>). Resolve
///         <see cref="IBackupService"/> from a DI scope when routines depend
///         on scoped services.
///     </para>
/// </remarks>
public interface IBackupRoutine {
    /// <summary>
    ///     Gets the stable, non-localized identifier of the routine. It names
    ///     the routine's folder inside the archive, so it must never change
    ///     once archives exist. Lowercase letters, digits, <c>.</c>,
    ///     <c>_</c> and <c>-</c>; starts and ends with a letter or digit;
    ///     at most 64 characters; not a Windows reserved device name.
    /// </summary>
    string Key { get; }

    /// <summary>
    ///     Gets the execution order. Lower values run first (ties are
    ///     ordered by <see cref="Key"/>) on backup and restore; rollback runs
    ///     in reverse.
    /// </summary>
    int Order { get; }

    /// <summary>
    ///     Writes the routine's state into
    ///     <see cref="BackupRoutineContext.WorkingDirectoryPath"/>.
    /// </summary>
    /// <param name="context">
    ///     The backup context.
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation.
    /// </param>
    /// <returns>
    ///     <see cref="Nothing"/> on success; the errors otherwise.
    /// </returns>
    Task<EmptyResult> BackupAsync(BackupRoutineContext context, CancellationToken cancellationToken);

    /// <summary>
    ///     Replaces the routine's state with the content of
    ///     <see cref="RestoreRoutineContext.WorkingDirectoryPath"/>.
    /// </summary>
    /// <param name="context">
    ///     The restore context.
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation. Rollbacks receive
    ///     <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    ///     <see cref="Nothing"/> on success; the errors otherwise.
    /// </returns>
    Task<EmptyResult> RestoreAsync(RestoreRoutineContext context, CancellationToken cancellationToken);
}
