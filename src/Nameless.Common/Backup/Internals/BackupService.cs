using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nameless.Compression;
using Nameless.Compression.Internals;
using Nameless.ObjectModel;

namespace Nameless.Backup.Internals;

/// <summary>
///     Default <see cref="IBackupService"/> implementation.
/// </summary>
internal sealed class BackupService : IBackupService {
    private const string BACKUP_OPERATION = "backup";
    private const string RESTORE_OPERATION = "restore";
    private const string ROLLBACK_OPERATION = "rollback";

    private const string EXTRACTED_DIRECTORY_NAME = "archive";
    private const string SNAPSHOT_DIRECTORY_NAME = "snapshot";

    private readonly BackupRoutineCatalog _catalog;
    private readonly ICompressor _compressor;
    private readonly BackupArchiveReader _reader;
    private readonly BackupOperationGate _gate;
    private readonly BackupOptions _options;
    private readonly string _workingDirectoryPath;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BackupService> _logger;

    /// <exception cref="InvalidOperationException">
    ///     if a routine has an invalid key, or two routines share a key.
    /// </exception>
    public BackupService(IEnumerable<IBackupRoutine> routines, ICompressor compressor, BackupOperationGate gate, TimeProvider timeProvider, IOptions<BackupOptions> options, ILogger<BackupService> logger) {
        _catalog = new BackupRoutineCatalog(Throws.When.Null(routines));
        _compressor = Throws.When.Null(compressor);
        _reader = new BackupArchiveReader(compressor);
        _gate = Throws.When.Null(gate);
        _options = Throws.When.Null(options).Value;

        // resolved once, so paths in errors and logs are absolute
        _workingDirectoryPath = SysPath.GetFullPath(_options.WorkingDirectoryPath);
        _timeProvider = Throws.When.Null(timeProvider);
        _logger = Throws.When.Null(logger);
    }

    /// <inheritdoc />
    public async Task<BackupResult> BackupAsync(BackupRequest request, IProgress<BackupProgress>? progress, CancellationToken cancellationToken) {
        Throws.When.Null(request);

        using var lease = _gate.TryEnter();

        if (lease is null) { return OperationInProgress(); }

        if (_catalog.Routines.Count == 0) { return NoRoutines(); }

        var destination = SysPath.GetFullPath(request.DestinationFilePath);

        if (!request.Overwrite && SysFile.Exists(destination)) {
            return Error.Conflict($"The archive '{destination}' already exists.");
        }

        progress?.Report(new BackupProgress(BackupStage.Preparing));
        Log.BackupStarted(_logger, _catalog.Routines.Count, destination);

        var started = _timeProvider.GetTimestamp();
        var createdAt = _timeProvider.GetUtcNow();
        var staging = Staging.Create(_workingDirectoryPath, BACKUP_OPERATION, createdAt);

        BackupResult result;

        try {
            result = await BackupCoreAsync(
                destination,
                request.Overwrite,
                staging.Path,
                createdAt,
                progress,
                cancellationToken
            ).SkipContextSync();
        }
        finally {
            progress?.Report(new BackupProgress(BackupStage.CleaningUp));
            CleanUp(staging);
        }

        if (result.Success) {
            Log.BackupCompleted(
                _logger,
                destination,
                _timeProvider.GetElapsedTime(started).TotalMilliseconds
            );
            progress?.Report(new BackupProgress(BackupStage.Completed));
        }
        else { Log.BackupFailed(_logger, destination, Describe(result.Errors)); }

        return result;
    }

    /// <inheritdoc />
    public async Task<RestoreResult> RestoreAsync(RestoreRequest request, IProgress<BackupProgress>? progress, CancellationToken cancellationToken) {
        Throws.When.Null(request);

        if (!ArchiveChecksum.IsValid(request.Checksum)) {
            return Error.Validation(
                $"The expected checksum is malformed. {ArchiveChecksum.InvalidChecksumMessage}"
            );
        }

        using var lease = _gate.TryEnter();

        if (lease is null) { return OperationInProgress(); }

        if (_catalog.Routines.Count == 0) { return NoRoutines(); }

        var archive = SysPath.GetFullPath(request.ArchiveFilePath);

        if (!SysFile.Exists(archive)) {
            return Error.Missing($"The archive '{archive}' was not found.");
        }

        progress?.Report(new BackupProgress(BackupStage.Preparing));

        Log.RestoreStarted(_logger, archive);

        var started = _timeProvider.GetTimestamp();
        var staging = Staging.Create(_workingDirectoryPath,
            RESTORE_OPERATION,
            _timeProvider.GetUtcNow()
        );

        RestoreResult result;

        try {
            result = await RestoreCoreAsync(
                archive,
                request.Checksum,
                staging,
                progress,
                cancellationToken
            ).SkipContextSync();
        }
        finally {
            progress?.Report(new BackupProgress(BackupStage.CleaningUp));
            CleanUp(staging);
        }

        if (result.Success) {
            Log.RestoreCompleted(
                _logger,
                archive,
                _timeProvider.GetElapsedTime(started).TotalMilliseconds,
                result.Value.RestoredRoutineKeys.Count,
                result.Value.SkippedRoutineKeys.Count
            );
            progress?.Report(new BackupProgress(BackupStage.Completed));
        }
        else { Log.RestoreFailed(_logger, archive, Describe(result.Errors)); }

        return result;
    }

    private async Task<BackupResult> BackupCoreAsync(string destination, bool overwrite, string stagingPath, DateTimeOffset createdAt, IProgress<BackupProgress>? progress, CancellationToken cancellationToken) {
        var payload = SysPath.Combine(stagingPath, BackupManifest.PayloadDirectoryName);

        var manifest = await RunBackupsAsync(
            _catalog.Routines,
            payload,
            createdAt,
            BackupStage.BackingUp,
            progress,
            cancellationToken
        ).SkipContextSync();

        if (!manifest.Success) { return manifest.Errors; }

        try {
            await BackupManifestSerializer.WriteAsync(
                manifest.Value,
                SysPath.Combine(payload, BackupManifest.FileName),
                cancellationToken
            ).SkipContextSync();

            progress?.Report(new BackupProgress(BackupStage.Compressing));

            // the compressor writes into a temporary file and moves it into
            // place, so a failed or cancelled backup never leaves a partial
            // archive at the destination
            var compression = await _compressor.CompressDirectoryAsync(
                payload,
                globPattern: null,
                destination,
                _options.CompressionLevel,
                overwrite,
                cancellationToken
            ).SkipContextSync();

            return new BackupMetadata(
                compression.FilePath,
                compression.Checksum,
                compression.Algorithm,
                createdAt,
                [.. _catalog.Routines.Select(routine => routine.Key)]
            );
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
            return Error.Failure($"Writing the archive '{destination}' failed: {ex.Message}", ex: ex);
        }
    }

    private async Task<RestoreResult> RestoreCoreAsync(string archive, string? checksum, Staging staging, IProgress<BackupProgress>? progress, CancellationToken cancellationToken) {
        progress?.Report(new BackupProgress(BackupStage.Extracting));

        var extracted = SysPath.Combine(staging.Path, EXTRACTED_DIRECTORY_NAME);
        var extraction = await _reader.ExtractAsync(
            archive,
            checksum,
            extracted,
            cancellationToken
        ).SkipContextSync();

        if (!extraction.Success) { return extraction.Errors; }

        progress?.Report(new BackupProgress(BackupStage.Validating));

        var payload = SysPath.Combine(extracted, BackupManifest.PayloadDirectoryName);
        var manifest = await BackupArchiveReader.ReadManifestAsync(
            extracted,
            cancellationToken
        ).SkipContextSync();

        if (!manifest.Success) { return manifest.Errors; }

        var archivedKeys = manifest.Value.Routines.Select(routine => routine.Key).ToHashSet(StringComparer.Ordinal);
        var unknownKeys = archivedKeys.Where(key => !_catalog.TryGet(key, out _)).Order(StringComparer.Ordinal).ToArray();

        if (unknownKeys.Length > 0) {
            return Error.Missing($"The archive holds routines that are not registered: {string.Join(", ", unknownKeys)}. Nothing was restored.");
        }

        IBackupRoutine[] restoring = [.. _catalog.Routines.Where(routine => archivedKeys.Contains(routine.Key))];
        string[] skipped = [.. _catalog.Routines.Where(routine => !archivedKeys.Contains(routine.Key)).Select(routine => routine.Key)];

        var snapshotRoot = SysPath.Combine(staging.Path, SNAPSHOT_DIRECTORY_NAME);
        var snapshotCreatedAt = _timeProvider.GetUtcNow();
        var snapshot = await RunBackupsAsync(
            restoring,
            snapshotRoot,
            snapshotCreatedAt,
            BackupStage.CreatingSafetySnapshot,
            progress,
            cancellationToken
        ).SkipContextSync();

        if (!snapshot.Success) { return snapshot.Errors; }

        var restore = await RunRestoresAsync(
            restoring,
            payload,
            manifest.Value.CreatedAt,
            new Snapshot(snapshotRoot, snapshotCreatedAt, staging),
            progress,
            cancellationToken
        ).SkipContextSync();

        if (!restore.Success) { return restore.Errors; }

        return new RestoreMetadata(
            manifest.Value.CreatedAt,
            [.. restoring.Select(routine => routine.Key)],
            skipped
        );
    }

    /// <summary>
    ///     Runs the routines' backup into <c>root/key</c> folders and
    ///     describes the result.
    /// </summary>
    private async Task<BackupManifestResult> RunBackupsAsync(IReadOnlyList<IBackupRoutine> routines, string root, DateTimeOffset createdAt, BackupStage stage, IProgress<BackupProgress>? progress, CancellationToken cancellationToken) {
        var described = new List<BackupManifestRoutine>(routines.Count);

        foreach (var routine in routines) {
            cancellationToken.ThrowIfCancellationRequested();

            var directory = SysDirectory.CreateDirectory(SysPath.Combine(root, routine.Key)).FullName;

            progress?.Report(new BackupProgress(stage, routine.Key));

            var result = await InvokeAsync(
                routine,
                BACKUP_OPERATION,
                () => routine.BackupAsync(
                    new BackupRoutineContext(directory, createdAt),
                    cancellationToken
                ),
                cancellationToken
            ).SkipContextSync();

            if (!result.Success) { return result.Errors; }

            var description = await BackupManifestBuilder.DescribeAsync(
                routine.Key,
                routine.Order,
                directory,
                cancellationToken
            ).SkipContextSync();

            if (!description.Success) { return description.Errors; }

            described.Add(description.Value);
        }

        return new BackupManifest(BackupManifest.CurrentFormatVersion, createdAt, described);
    }

    /// <summary>
    ///     Restores the routines in order. On failure or cancellation, the
    ///     routines already attempted are rolled back from the snapshot.
    /// </summary>
    private async Task<EmptyResult> RunRestoresAsync(IReadOnlyList<IBackupRoutine> routines, string payload, DateTimeOffset backupCreatedAt, Snapshot snapshot, IProgress<BackupProgress>? progress, CancellationToken cancellationToken) {
        var attempted = new List<IBackupRoutine>(routines.Count);

        try {
            foreach (var routine in routines) {
                cancellationToken.ThrowIfCancellationRequested();

                attempted.Add(routine);

                // a routine with no files may have no folder in the archive
                var directory = SysDirectory.CreateDirectory(SysPath.Combine(payload, routine.Key)).FullName;

                progress?.Report(new BackupProgress(BackupStage.Restoring, routine.Key));

                var result = await InvokeAsync(
                    routine,
                    RESTORE_OPERATION,
                    () => routine.RestoreAsync(
                        new RestoreRoutineContext(directory, backupCreatedAt, IsRollback: false),
                        cancellationToken
                    ),
                    cancellationToken
                ).SkipContextSync();

                if (result.Success) { continue; }

                Log.RollingBack(_logger, routine.Key, attempted.Count);

                var rollback = await RollbackAsync(
                    attempted,
                    snapshot,
                    progress
                ).SkipContextSync();

                return rollback.Success
                    ? result.Errors
                    : KeepSnapshot(snapshot, result.Errors, rollback.Errors);
            }

            return Nothing.Value;
        }
        // cancellation, or anything thrown outside a routine (e.g. by the
        // progress receiver) once live state may have changed: roll back,
        // then let the exception surface
        catch (Exception ex) when (attempted.Count > 0) {
            Log.RollingBack(_logger, attempted[^1].Key, attempted.Count);

            var rollback = await RollbackAsync(
                attempted,
                snapshot,
                progress
            ).SkipContextSync();

            if (rollback.Success) { throw; }

            var reason = ex is OperationCanceledException
                ? Error.Failure("The restore was cancelled.")
                : Error.Failure($"The restore was interrupted: {ex.Message}", ex: ex);

            return KeepSnapshot(snapshot, [reason], rollback.Errors);
        }
    }

    /// <summary>
    ///     Restores the attempted routines from the snapshot, in reverse
    ///     order. Not cancellable; every routine is tried even if one fails.
    /// </summary>
    private async Task<EmptyResult> RollbackAsync(List<IBackupRoutine> attempted, Snapshot snapshot, IProgress<BackupProgress>? progress) {
        var errors = new List<Error>();

        for (var index = attempted.Count - 1; index >= 0; index--) {
            var routine = attempted[index];
            var directory = SysPath.Combine(snapshot.Path, routine.Key);

            ReportDuringRollback(progress, new BackupProgress(BackupStage.RollingBack, routine.Key));

            var result = await InvokeAsync(
                routine,
                ROLLBACK_OPERATION,
                () => routine.RestoreAsync(
                    new RestoreRoutineContext(directory, snapshot.CreatedAt, IsRollback: true),
                    CancellationToken.None
                ),
                CancellationToken.None
            ).SkipContextSync();

            if (!result.Success) {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Count == 0
            ? Nothing.Value
            : errors.ToArray();
    }

    /// <summary>
    ///     A failing progress receiver must not stop a rollback.
    /// </summary>
    private void ReportDuringRollback(IProgress<BackupProgress>? progress, BackupProgress value) {
        try {
            progress?.Report(value);
        }
        // the receiver is caller code: whatever it throws is logged and ignored
        catch (Exception ex) {
            Log.ProgressReportFailed(_logger, ex, value.Stage);
        }
    }

    /// <summary>
    ///     Moves the snapshot out of the staging directory so cleanup keeps
    ///     it, and builds the errors naming where it is.
    /// </summary>
    private Error[] KeepSnapshot(Snapshot snapshot, Error[] restoreErrors, Error[] rollbackErrors) {
        var kept = SysPath.Combine(
            _workingDirectoryPath,
            Staging.CreateName(ROLLBACK_OPERATION, _timeProvider.GetUtcNow())
        );

        try {
            SysDirectory.Move(snapshot.Path, kept);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
            Log.SnapshotMoveFailed(_logger, ex, snapshot.Path);

            snapshot.Staging.Keep = true;
            kept = snapshot.Path;
        }

        Log.RollbackFailed(_logger, kept);

        return [
            Error.Failure($"The restore failed and so did the rollback. The safety snapshot was kept at '{kept}' for manual recovery."),
            .. restoreErrors,
            .. rollbackErrors
        ];
    }

    private async Task<EmptyResult> InvokeAsync(IBackupRoutine routine, string operation, Func<Task<EmptyResult>> call, CancellationToken cancellationToken) {
        EmptyResult result;

        try {
            result = await call().SkipContextSync();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        }
        // a routine is user code: any exception becomes a failure
        catch (Exception ex) {
            Log.RoutineThrew(_logger, ex, routine.Key, operation);

            return Error.Failure($"Backup routine '{routine.Key}' threw during its {operation}: {ex.Message}", ex: ex);
        }

        if (!result.Success) {
            Log.RoutineFailed(_logger, routine.Key, operation, Describe(result.Errors));
        }

        return result;
    }

    private void CleanUp(Staging staging) {
        if (staging.Keep) { return; }

        try {
            SysDirectory.Delete(staging.Path, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
            Log.CleanupFailed(_logger, ex, staging.Path);
        }
    }

    private static Error OperationInProgress() {
        return Error.Conflict("Another backup or restore is in progress.");
    }

    private static Error NoRoutines() {
        return Error.Validation("No backup routine is registered.");
    }

    private static string Describe(Error[] errors) {
        return string.Join("; ", errors.Select(error => error.Flatten));
    }

    /// <summary>
    ///     A per-operation directory under the working directory.
    /// </summary>
    private sealed class Staging {
        public string Path { get; }

        /// <summary>
        ///     Whether cleanup must leave the directory in place (it holds a
        ///     safety snapshot that could not be moved out).
        /// </summary>
        public bool Keep { get; set; }

        private Staging(string path) {
            Path = path;
        }

        public static Staging Create(string workingDirectoryPath, string operation, DateTimeOffset timestamp) {
            var path = SysPath.Combine(workingDirectoryPath, CreateName(operation, timestamp));

            SysDirectory.CreateDirectory(path);

            return new Staging(path);
        }

        public static string CreateName(string operation, DateTimeOffset timestamp) {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{operation}-{timestamp.UtcDateTime:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..8]}"
            );
        }
    }

    private sealed record Snapshot(string Path, DateTimeOffset CreatedAt, Staging Staging);
}
