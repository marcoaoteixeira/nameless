using Microsoft.Extensions.Logging;
using Nameless.Windows.Messaging;

namespace Nameless.Windows.BackupRecovery;

public abstract class BackupRecoveryRoutineBase<TSelf> : IBackupRecoveryRoutine
    where TSelf : BackupRecoveryRoutineBase<TSelf> {
    private readonly IMessenger _messenger;

    public virtual string Name => GetType().Name;

    protected ILogger<TSelf> Logger { get; }

    protected BackupRecoveryRoutineBase(IMessenger messenger, ILogger<TSelf> logger) {
        _messenger = messenger;

        Logger = logger;
    }

    public abstract Task<BackupOutput> BackupAsync(BackupInput input, CancellationToken cancellationToken);

    public abstract Task<RestoreOutput> RestoreAsync(RestoreInput input, CancellationToken cancellationToken);

    protected Task NotifyInformationAsync(string content) {
        return _messenger.PublishInformationAsync<DisasterRecoveryRoutineMessage>(
            content,
            title: Name
        );
    }

    protected Task NotifySuccessAsync(string content) {
        return _messenger.PublishSuccessAsync<DisasterRecoveryRoutineMessage>(
            content,
            title: Name
        );
    }

    protected Task NotifyWarningAsync(string content) {
        return _messenger.PublishWarningAsync<DisasterRecoveryRoutineMessage>(
            content,
            title: Name
        );
    }

    protected Task NotifyFailureAsync(string content) {
        return _messenger.PublishFailureAsync<DisasterRecoveryRoutineMessage>(
            content,
            title: Name
        );
    }
}