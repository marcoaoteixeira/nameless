using Nameless.ObjectModel;

namespace Nameless.Backup.Infrastructure;

/// <summary>
///     What a <see cref="TestRoutine"/> does when one of its methods is called.
/// </summary>
public enum RoutineBehavior
{
    Succeed,
    Fail,
    Throw,
    Cancel
}

/// <summary>
///     Shared, ordered record of routine calls ("key:backup", "key:restore",
///     "key:rollback").
/// </summary>
internal sealed class CallLog
{
    private readonly List<string> _calls = [];

    public IReadOnlyList<string> Calls => _calls;

    public void Add(string call)
    {
        lock (_calls) { _calls.Add(call); }
    }
}

/// <summary>
///     Backup routine over a "live" directory: backup copies the live
///     directory into the working directory, restore replaces the live
///     directory with the working directory content.
/// </summary>
internal sealed class TestRoutine : IBackupRoutine
{
    private readonly CallLog _log;

    public string Key { get; }

    public int Order { get; }

    public string LiveDirectoryPath { get; }

    public RoutineBehavior OnBackup { get; set; } = RoutineBehavior.Succeed;

    public RoutineBehavior OnRestore { get; set; } = RoutineBehavior.Succeed;

    public RoutineBehavior OnRollback { get; set; } = RoutineBehavior.Succeed;

    /// <summary>
    ///     Runs inside the backup call (before the behavior is applied).
    /// </summary>
    public Action? DuringBackup { get; set; }

    /// <summary>
    ///     Runs inside the restore call (before the behavior is applied).
    /// </summary>
    public Action? DuringRestore { get; set; }

    public List<BackupRoutineContext> BackupContexts { get; } = [];

    public List<RestoreRoutineContext> RestoreContexts { get; } = [];

    public TestRoutine(string key, int order, string liveDirectoryPath, CallLog log)
    {
        Key = key;
        Order = order;
        LiveDirectoryPath = liveDirectoryPath;
        _log = log;

        SysDirectory.CreateDirectory(liveDirectoryPath);
    }

    public Task<EmptyResult> BackupAsync(BackupRoutineContext context, CancellationToken cancellationToken)
    {
        _log.Add($"{Key}:backup");
        BackupContexts.Add(context);
        DuringBackup?.Invoke();

        return Apply(OnBackup, cancellationToken, () => CopyDirectory(LiveDirectoryPath, context.WorkingDirectoryPath));
    }

    public Task<EmptyResult> RestoreAsync(RestoreRoutineContext context, CancellationToken cancellationToken)
    {
        _log.Add(context.IsRollback ? $"{Key}:rollback" : $"{Key}:restore");
        RestoreContexts.Add(context);

        if (!context.IsRollback)
        {
            DuringRestore?.Invoke();

            // a failing restore leaves a partial write behind, so rollback
            // tests can prove the live state is really put back
            if (OnRestore != RoutineBehavior.Succeed)
            {
                SysFile.WriteAllText(SysPath.Combine(LiveDirectoryPath, "partial.txt"), "partial");
            }
        }

        return Apply(context.IsRollback ? OnRollback : OnRestore, cancellationToken, () =>
        {
            SysDirectory.Delete(LiveDirectoryPath, recursive: true);
            CopyDirectory(context.WorkingDirectoryPath, LiveDirectoryPath);
        });
    }

    private Task<EmptyResult> Apply(RoutineBehavior behavior, CancellationToken cancellationToken, Action work)
    {
        switch (behavior)
        {
            case RoutineBehavior.Fail:
                return Task.FromResult<EmptyResult>(Error.Failure($"{Key} failed"));

            case RoutineBehavior.Throw:
                throw new InvalidOperationException($"{Key} threw");

            case RoutineBehavior.Cancel:
                throw new OperationCanceledException($"{Key} cancelled", cancellationToken);

            default:
                work();

                return Task.FromResult<EmptyResult>(Nothing.Value);
        }
    }

    public static void CopyDirectory(string source, string destination)
    {
        SysDirectory.CreateDirectory(destination);

        foreach (var file in SysDirectory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = SysPath.Combine(destination, SysPath.GetRelativePath(source, file));

            SysDirectory.CreateDirectory(SysPath.GetDirectoryName(target)!);
            SysFile.Copy(file, target, overwrite: true);
        }
    }
}
