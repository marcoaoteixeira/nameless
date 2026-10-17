using Nameless.ObjectModel;

namespace Nameless.Backup;

/// <summary>
///     Backs up every registered <see cref="IBackupRoutine"/> into a single
///     archive, and restores them from it.
/// </summary>
/// <remarks>
///     <para>
///         Archives hold the application data as is (not encrypted); treat
///         them as sensitive. The per-file SHA-256 hashes of the manifest
///         detect corruption only. To detect tampering, configure
///         <c>ZipCompressorOptions.HmacKey</c> and pass the checksum of
///         <see cref="BackupMetadata"/>, kept in a trusted place, to
///         <see cref="RestoreRequest.Checksum"/>.
///     </para>
///     <para>
///         Only one backup or restore runs at a time per service provider;
///         a concurrent call fails right away with an
///         <see cref="ErrorType.Conflict"/> error.
///     </para>
/// </remarks>
public interface IBackupService {
    /// <summary>
    ///     Runs every routine's backup, in order, and writes the archive.
    ///     All or nothing: if a routine fails, no archive is written.
    /// </summary>
    /// <param name="request">
    ///     The backup parameters.
    /// </param>
    /// <param name="progress">
    ///     Optional progress receiver.
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation.
    /// </param>
    /// <returns>
    ///     The archive information, or the errors.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     if the operation is cancelled; nothing is written.
    /// </exception>
    Task<BackupResult> BackupAsync(BackupRequest request, IProgress<BackupProgress>? progress, CancellationToken cancellationToken);

    /// <summary>
    ///     Validates the archive, takes a safety snapshot of the current
    ///     state, then runs every routine's restore, in order. If a routine
    ///     fails (or the operation is cancelled), the routines already run
    ///     are rolled back from the snapshot, in reverse order.
    /// </summary>
    /// <param name="request">
    ///     The restore parameters.
    /// </param>
    /// <param name="progress">
    ///     Optional progress receiver.
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation.
    /// </param>
    /// <returns>
    ///     The restore information, or the errors. If the rollback fails
    ///     too, the errors name the directory where the safety snapshot
    ///     was kept for manual recovery.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     if the operation is cancelled and the rollback (when needed)
    ///     succeeded.
    /// </exception>
    Task<RestoreResult> RestoreAsync(RestoreRequest request, IProgress<BackupProgress>? progress, CancellationToken cancellationToken);
}
