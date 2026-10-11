using System.IO;
using Nameless.Application;
using Nameless.Compression;
using Nameless.Mediator.Requests;
using Nameless.ObjectModel;
using Nameless.Windows.BackupRecovery;

namespace Nameless.Windows.UseCases.Restore;

public class PerformApplicationRestoreRequestHandler : IRequestHandler<PerformApplicationRestoreRequest, PerformApplicationRestoreResponse> {
    private readonly IApplicationContext _applicationContext;
    private readonly IBackupRecoveryRoutine[] _disasterRecoveryRoutines;
    private readonly ICompressor _compressor;

    public PerformApplicationRestoreRequestHandler(
        IApplicationContext applicationContext,
        ICompressor compressor,
        IEnumerable<IBackupRecoveryRoutine> disasterRecoveryRoutines) {
        _applicationContext = applicationContext;
        _compressor = compressor;
        _disasterRecoveryRoutines = [.. disasterRecoveryRoutines];
    }

    public async Task<PerformApplicationRestoreResponse> HandleAsync(PerformApplicationRestoreRequest request, CancellationToken cancellationToken) {
        var backupFilePath = GetBackupFilePath(request.Timestamp);
        if (backupFilePath.Failure) { return backupFilePath.Errors; }

        var backupDirectoryPath = await DecompressBackupFileAsync(
            backupFilePath.Value,
            cancellationToken
        ).SkipContextSync();
        if (backupDirectoryPath.Failure) { return backupDirectoryPath.Errors; }

        // execute the restore routines
        var input = new RestoreInput {
            SourceDirectoryPath = backupDirectoryPath.Value,
            DestinationDirectoryPath = FolderStructure.DatabaseDirectoryName
        };
        var routines = _disasterRecoveryRoutines.Select(
            routine => routine.RestoreAsync(input, cancellationToken)
        );

        var results = await Task.WhenAll(routines).SkipContextSync();

        foreach (var result in results) {
            if (result.Success) { continue; }

            // notify of possible errors.
        }

        throw new NotImplementedException();
    }

    private Result<string> GetBackupFilePath(DateTimeOffset timestamp) {
        // backup files should always be in the "backups" directory.
        var backupFileName = string.Format(BackupFileNamePattern, timestamp);
        var backupDirectory = _applicationContext.ApplicationDataFileProvider.GetBackupDirectory();
        var backupFile = _applicationContext.ApplicationDataFileProvider.GetFile(
            Path.Combine(backupDirectory.Path, backupFileName)
        );

        if (!backupFile.Exists) {
            return Error.Missing("Backup file not found");
        }

        return backupFile.Path;
    }

    private async Task<Result<string>> DecompressBackupFileAsync(string backupFilePath, CancellationToken cancellationToken) {
        // decompress file into the temporary directory
        var request = new DecompressRequest(backupFilePath) {
            DestinationDirectoryPath = _applicationContext.ApplicationDataFileProvider.GetTemporaryDirectory().Path
        };

        var response = await _compressor.DecompressAsync(request, cancellationToken)
                                        .SkipContextSync();

        return response.Match<Result<string>>(
            onSuccess: value => value.DirectoryPath,
            onFailure: errors => errors
        );
    }
}