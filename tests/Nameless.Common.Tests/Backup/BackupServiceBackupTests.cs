using System.IO.Compression;
using Nameless.Backup.Infrastructure;
using Nameless.ObjectModel;

namespace Nameless.Backup;

[IntegrationTest]
public sealed class BackupServiceBackupTests : IDisposable
{
    private readonly BackupServiceFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<BackupResult> BackupAsync(bool overwrite = false, IProgress<BackupProgress>? progress = null, CancellationToken? cancellationToken = null)
    {
        return _fixture.CreateService().BackupAsync(
            new BackupRequest { DestinationFilePath = _fixture.ArchivePath, Overwrite = overwrite },
            progress,
            cancellationToken ?? Token
        );
    }

    [Fact]
    public async Task Backup_writes_archive_with_manifest_and_one_folder_per_routine()
    {
        _fixture.AddRoutine("files", order: 1, ("a/b.txt", "bee"));
        _fixture.AddRoutine("db", order: 0, ("data.txt", "rows"));

        var result = await BackupAsync();

        Assert.True(result.Success);
        Assert.Equal(SysPath.GetFullPath(_fixture.ArchivePath), result.Value.FilePath);
        Assert.Equal(BackupServiceFixture.Now, result.Value.CreatedAt);
        Assert.Equal(["db", "files"], result.Value.RoutineKeys);
        Assert.Matches("^[0-9a-f]{64}$", result.Value.Checksum);

        using var archive = ZipFile.OpenRead(_fixture.ArchivePath);
        var entries = archive.Entries.Where(entry => !entry.FullName.EndsWith('/')).Select(entry => entry.FullName).Order(StringComparer.Ordinal);

        Assert.Equal(["backup/db/data.txt", "backup/files/a/b.txt", "backup/manifest.json"], entries);
    }

    [Fact]
    public async Task Routines_run_in_order_with_their_own_working_directory()
    {
        var b = _fixture.AddRoutine("b", order: 1);
        var a = _fixture.AddRoutine("a", order: 1);
        var c = _fixture.AddRoutine("c", order: 0);

        await BackupAsync();

        Assert.Equal(["c:backup", "a:backup", "b:backup"], _fixture.Calls.Calls);

        var contexts = new[] { a, b, c }.Select(routine => Assert.Single(routine.BackupContexts)).ToArray();

        Assert.All(contexts, context => Assert.Equal(BackupServiceFixture.Now, context.CreatedAt));
        Assert.Equal(["a", "b", "c"], contexts.Select(context => SysPath.GetFileName(context.WorkingDirectoryPath)));
        Assert.Equal(3, contexts.Select(context => context.WorkingDirectoryPath).Distinct().Count());
    }

    [Fact]
    public async Task Staging_is_deleted_after_backup()
    {
        _fixture.AddRoutine("db", files: ("data.txt", "rows"));

        await BackupAsync();

        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Failing_routine_stops_the_backup_and_writes_nothing()
    {
        _fixture.AddRoutine("a", order: 0).OnBackup = RoutineBehavior.Fail;
        _fixture.AddRoutine("b", order: 1);

        var result = await BackupAsync();

        Assert.False(result.Success);
        Assert.Equal("a failed", result.Errors[0].Message);
        Assert.Equal(["a:backup"], _fixture.Calls.Calls);
        Assert.False(SysFile.Exists(_fixture.ArchivePath));
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Throwing_routine_is_reported_as_a_failure()
    {
        _fixture.AddRoutine("a").OnBackup = RoutineBehavior.Throw;

        var result = await BackupAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Failure, result.Errors[0].Type);
        Assert.IsType<InvalidOperationException>(result.Errors[0].Exception);
        Assert.Contains("'a'", result.Errors[0].Message);
        Assert.False(SysFile.Exists(_fixture.ArchivePath));
    }

    [Fact]
    public async Task Existing_destination_without_overwrite_is_a_conflict_and_runs_nothing()
    {
        _fixture.AddRoutine("a");
        _fixture.Temp.CreateFile(SysPath.Combine("out", "app.backup"), "previous");

        var result = await BackupAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Conflict, result.Errors[0].Type);
        Assert.Empty(_fixture.Calls.Calls);
        Assert.Equal("previous", await SysFile.ReadAllTextAsync(_fixture.ArchivePath, Token));
    }

    [Fact]
    public async Task Existing_destination_with_overwrite_is_replaced()
    {
        _fixture.AddRoutine("a", files: ("x.txt", "x"));
        _fixture.Temp.CreateFile(SysPath.Combine("out", "app.backup"), "previous");

        var result = await BackupAsync(overwrite: true);

        Assert.True(result.Success);
        using var archive = ZipFile.OpenRead(_fixture.ArchivePath);
        Assert.Contains(archive.Entries, entry => entry.FullName == "backup/manifest.json");
    }

    [Fact]
    public async Task Cancellation_throws_and_leaves_nothing_behind()
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token);
        _fixture.AddRoutine("a", order: 0).DuringBackup = cts.Cancel;
        _fixture.AddRoutine("b", order: 1);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BackupAsync(cancellationToken: cts.Token));

        Assert.Equal(["a:backup"], _fixture.Calls.Calls);
        Assert.False(SysFile.Exists(_fixture.ArchivePath));
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task No_routines_is_a_validation_error()
    {
        var result = await BackupAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }

    [Fact]
    public async Task Concurrent_operation_is_a_conflict()
    {
        _fixture.AddRoutine("a");
        using var held = _fixture.Gate.TryEnter();

        var result = await BackupAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Conflict, result.Errors[0].Type);
        Assert.Empty(_fixture.Calls.Calls);
    }

    [Fact]
    public async Task Gate_is_released_after_the_operation()
    {
        _fixture.AddRoutine("a");

        await BackupAsync();

        using var lease = _fixture.Gate.TryEnter();
        Assert.NotNull(lease);
    }

    [Fact]
    public async Task Progress_is_reported_in_order()
    {
        _fixture.AddRoutine("a", order: 0);
        _fixture.AddRoutine("b", order: 1);
        var progress = new ProgressRecorder();

        await BackupAsync(progress: progress);

        Assert.Equal(
            [
                new BackupProgress(BackupStage.Preparing),
                new BackupProgress(BackupStage.BackingUp, "a"),
                new BackupProgress(BackupStage.BackingUp, "b"),
                new BackupProgress(BackupStage.Compressing),
                new BackupProgress(BackupStage.CleaningUp),
                new BackupProgress(BackupStage.Completed)
            ],
            progress.Reports
        );
    }

    [Fact]
    public async Task Null_request_throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _fixture.CreateService().BackupAsync(null!, null, Token));
    }

    [Fact]
    public void Duplicate_routine_keys_fail_on_construction()
    {
        _fixture.AddRoutine("a");
        _fixture.AddRoutine("a");

        Assert.Throws<InvalidOperationException>(_fixture.CreateService);
    }
}
