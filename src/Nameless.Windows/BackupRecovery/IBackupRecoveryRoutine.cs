namespace Nameless.Windows.BackupRecovery;

public interface IBackupRecoveryRoutine {
    string Name { get; }

    Task<BackupOutput> BackupAsync(BackupInput input, CancellationToken cancellationToken);

    Task<RestoreOutput> RestoreAsync(RestoreInput input, CancellationToken cancellationToken);
}
