using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Nameless.Backup.Infrastructure;
using Nameless.Backup.Internals;
using Nameless.ObjectModel;

namespace Nameless.Backup;

[IntegrationTest]
public sealed class BackupServiceRestoreTests : IDisposable
{
    private readonly BackupServiceFixture _fixture = new();

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private Task<RestoreResult> RestoreAsync(BackupService? service = null, string? checksum = null, IProgress<BackupProgress>? progress = null, CancellationToken? cancellationToken = null)
    {
        return (service ?? _fixture.CreateService()).RestoreAsync(
            new RestoreRequest { ArchiveFilePath = _fixture.ArchivePath, Checksum = checksum },
            progress,
            cancellationToken ?? Token
        );
    }

    /// <summary>
    ///     Routines a (order 0), b (order 1), c (order 2), backed up, then
    ///     their live state changed to "v2".
    /// </summary>
    private async Task<TestRoutine[]> BackedUpThenChangedAsync()
    {
        TestRoutine[] routines =
        [
            _fixture.AddRoutine("a", order: 0, ("a.txt", "a-v1")),
            _fixture.AddRoutine("b", order: 1, ("b.txt", "b-v1"), ("sub/b2.txt", "b2-v1")),
            _fixture.AddRoutine("c", order: 2, ("c.txt", "c-v1"))
        ];

        await _fixture.BackupAsync();

        foreach (var routine in routines)
        {
            BackupServiceFixture.WriteLive(routine, $"{routine.Key}.txt", $"{routine.Key}-v2");
            BackupServiceFixture.WriteLive(routine, "new.txt", "new");
        }

        return routines;
    }

    private int CallsSoFar => _fixture.Calls.Calls.Count;

    private IEnumerable<string> CallsSince(int index)
    {
        return _fixture.Calls.Calls.Skip(index);
    }

    // ---- Happy path -------------------------------------------------------

    [Fact]
    public async Task Restore_puts_back_the_backed_up_state()
    {
        var routines = await BackedUpThenChangedAsync();

        var result = await RestoreAsync();

        Assert.True(result.Success, result.Success ? null : result.Errors[0].Flatten);
        Assert.Equal(["a", "b", "c"], result.Value.RestoredRoutineKeys);
        Assert.Empty(result.Value.SkippedRoutineKeys);
        Assert.Equal(BackupServiceFixture.Now, result.Value.BackupCreatedAt);
        Assert.Equal(new Dictionary<string, string> { ["a.txt"] = "a-v1" }, BackupServiceFixture.ReadLive(routines[0]));
        Assert.Equal(new Dictionary<string, string> { ["b.txt"] = "b-v1", ["sub/b2.txt"] = "b2-v1" }, BackupServiceFixture.ReadLive(routines[1]));
        Assert.Equal(new Dictionary<string, string> { ["c.txt"] = "c-v1" }, BackupServiceFixture.ReadLive(routines[2]));
    }

    [Fact]
    public async Task Restore_takes_a_safety_snapshot_then_restores_in_order()
    {
        var routines = await BackedUpThenChangedAsync();
        var start = CallsSoFar;

        await RestoreAsync();

        Assert.Equal(["a:backup", "b:backup", "c:backup", "a:restore", "b:restore", "c:restore"], CallsSince(start));
        Assert.All(routines, routine =>
        {
            var context = Assert.Single(routine.RestoreContexts);
            Assert.False(context.IsRollback);
            Assert.Equal(BackupServiceFixture.Now, context.BackupCreatedAt);
            Assert.Equal(routine.Key, SysPath.GetFileName(context.WorkingDirectoryPath));
        });
    }

    [Fact]
    public async Task Archive_checksum_is_verified_when_given()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        var backup = await _fixture.BackupAsync();

        var result = await RestoreAsync(checksum: backup.Checksum);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Staging_is_deleted_after_restore()
    {
        await BackedUpThenChangedAsync();

        await RestoreAsync();

        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Registered_routine_missing_from_the_archive_is_skipped()
    {
        var a = _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        var late = new TestRoutine("late", 0, _fixture.Temp.Combine("live", "late"), _fixture.Calls);
        var start = CallsSoFar;

        var result = await RestoreAsync(_fixture.CreateService(a, late));

        Assert.True(result.Success);
        Assert.Equal(["a"], result.Value.RestoredRoutineKeys);
        Assert.Equal(["late"], result.Value.SkippedRoutineKeys);
        Assert.DoesNotContain(CallsSince(start), call => call.StartsWith("late:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Progress_is_reported_in_order()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        var progress = new ProgressRecorder();

        await RestoreAsync(progress: progress);

        Assert.Equal(
            [
                new BackupProgress(BackupStage.Preparing),
                new BackupProgress(BackupStage.Extracting),
                new BackupProgress(BackupStage.Validating),
                new BackupProgress(BackupStage.CreatingSafetySnapshot, "a"),
                new BackupProgress(BackupStage.Restoring, "a"),
                new BackupProgress(BackupStage.CleaningUp),
                new BackupProgress(BackupStage.Completed)
            ],
            progress.Reports
        );
    }

    // ---- Archive validation (nothing touched) -----------------------------

    private async Task AssertRejectedAsync(ErrorType expected, Func<Task<RestoreResult>>? restore = null)
    {
        var start = CallsSoFar;

        var result = await (restore ?? (() => RestoreAsync()))();

        Assert.False(result.Success);
        Assert.Equal(expected, result.Errors[0].Type);
        Assert.Empty(CallsSince(start));
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    private void EditArchive(Action<ZipArchive> edit)
    {
        using var archive = ZipFile.Open(_fixture.ArchivePath, ZipArchiveMode.Update);

        edit(archive);
    }

    private static void ReplaceEntry(ZipArchive archive, string name, string content)
    {
        archive.GetEntry(name)?.Delete();

        using var writer = new StreamWriter(archive.CreateEntry(name).Open());
        writer.Write(content);
    }

    [Fact]
    public async Task Missing_archive_is_missing()
    {
        _fixture.AddRoutine("a");

        await AssertRejectedAsync(ErrorType.Missing);
    }

    [Fact]
    public async Task Not_a_zip_is_rejected()
    {
        _fixture.AddRoutine("a");
        _fixture.Temp.CreateFile(SysPath.Combine("out", "app.backup"), "garbage");

        await AssertRejectedAsync(ErrorType.Validation);
    }

    [Fact]
    public async Task Wrong_checksum_is_rejected()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();

        await AssertRejectedAsync(ErrorType.Validation, () => RestoreAsync(checksum: new string('0', 64)));
    }

    public static TheoryData<string> MalformedChecksums => new()
    {
        "",
        "   ",
        "xyz",
        new string('a', 63),
        new string('a', 65),
        new string('g', 64),
    };

    [Theory]
    [MemberData(nameof(MalformedChecksums))]
    public async Task Malformed_checksum_is_rejected_before_anything_starts(string checksum)
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        var progress = new ProgressRecorder();

        await AssertRejectedAsync(ErrorType.Validation, () => RestoreAsync(checksum: checksum, progress: progress));

        Assert.Empty(progress.Reports);
    }

    [Fact]
    public async Task Tampered_file_is_rejected()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        EditArchive(archive => ReplaceEntry(archive, "backup/a/a.txt", "evil"));

        await AssertRejectedAsync(ErrorType.Validation);
    }

    [Fact]
    public async Task Unlisted_file_is_rejected()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        EditArchive(archive => ReplaceEntry(archive, "backup/a/extra.txt", "extra"));

        await AssertRejectedAsync(ErrorType.Validation);
    }

    [Fact]
    public async Task Extra_top_level_entry_is_rejected()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        EditArchive(archive => ReplaceEntry(archive, "other.txt", "other"));

        await AssertRejectedAsync(ErrorType.Validation);
    }

    [Fact]
    public async Task Archive_without_manifest_is_rejected()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        EditArchive(archive => archive.GetEntry("backup/manifest.json")!.Delete());

        await AssertRejectedAsync(ErrorType.Validation);
    }

    [Fact]
    public async Task Unknown_routine_in_the_archive_is_missing_and_nothing_runs()
    {
        var a = _fixture.AddRoutine("a", files: ("a.txt", "a"));
        _fixture.AddRoutine("b", files: ("b.txt", "b"));
        await _fixture.BackupAsync();

        await AssertRejectedAsync(ErrorType.Missing, () => RestoreAsync(_fixture.CreateService(a)));
    }

    [Fact]
    public async Task Unsafe_entry_is_rejected()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        EditArchive(archive => ReplaceEntry(archive, "../escape.txt", "evil"));

        await AssertRejectedAsync(ErrorType.Failure);
        Assert.False(SysFile.Exists(SysPath.Combine(_fixture.Options.WorkingDirectoryPath, "..", "escape.txt")));
    }

    // ---- Safety snapshot & rollback ---------------------------------------

    [Fact]
    public async Task Snapshot_failure_aborts_the_restore()
    {
        var routines = await BackedUpThenChangedAsync();
        routines[1].OnBackup = RoutineBehavior.Fail;
        var start = CallsSoFar;

        var result = await RestoreAsync();

        Assert.False(result.Success);
        Assert.Equal("b failed", result.Errors[0].Message);
        Assert.Equal(["a:backup", "b:backup"], CallsSince(start));
        Assert.Equal("a-v2", BackupServiceFixture.ReadLive(routines[0])["a.txt"]);
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Theory]
    [InlineData(RoutineBehavior.Fail)]
    [InlineData(RoutineBehavior.Throw)]
    public async Task Failed_restore_rolls_back_attempted_routines_in_reverse(RoutineBehavior behavior)
    {
        var routines = await BackedUpThenChangedAsync();
        var before = routines.Select(BackupServiceFixture.ReadLive).ToArray();
        routines[1].OnRestore = behavior;
        var start = CallsSoFar;

        var result = await RestoreAsync();

        Assert.False(result.Success);
        Assert.Equal(
            ["a:backup", "b:backup", "c:backup", "a:restore", "b:restore", "b:rollback", "a:rollback"],
            CallsSince(start)
        );
        Assert.All(routines.Take(2), routine => Assert.True(routine.RestoreContexts[^1].IsRollback));
        Assert.Equal(before, routines.Select(BackupServiceFixture.ReadLive));
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Failed_restore_returns_the_routine_errors()
    {
        var routines = await BackedUpThenChangedAsync();
        routines[1].OnRestore = RoutineBehavior.Fail;

        var result = await RestoreAsync();

        Assert.Equal("b failed", Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Rollback_failure_keeps_the_snapshot_and_reports_it()
    {
        var routines = await BackedUpThenChangedAsync();
        routines[1].OnRestore = RoutineBehavior.Fail;
        routines[0].OnRollback = RoutineBehavior.Fail;

        var result = await RestoreAsync();

        Assert.False(result.Success);
        var summary = result.Errors[0];
        Assert.Equal(ErrorType.Failure, summary.Type);
        Assert.Contains(result.Errors, error => error.Message == "b failed");
        Assert.Contains(result.Errors, error => error.Message == "a failed");

        var kept = Assert.Single(_fixture.WorkingDirectoryEntries());
        Assert.Contains(kept, summary.Message);
        Assert.True(SysDirectory.Exists(SysPath.Combine(kept, "a")));
        Assert.True(SysDirectory.Exists(SysPath.Combine(kept, "b")));
        Assert.Contains(_fixture.Logger.Records, record => record.Level == LogLevel.Critical);
    }

    [Fact]
    public async Task Relative_working_directory_is_reported_as_a_full_path()
    {
        var working = _fixture.Temp.Combine("relative-work");
        var relative = SysPath.GetRelativePath(Environment.CurrentDirectory, working);
        Assert.SkipWhen(SysPath.IsPathRooted(relative), "The temp directory is on another drive than the current directory.");

        var routines = await BackedUpThenChangedAsync();
        routines[1].OnRestore = RoutineBehavior.Fail;
        routines[0].OnRollback = RoutineBehavior.Fail;
        _fixture.Options.WorkingDirectoryPath = relative;

        var result = await RestoreAsync();

        var kept = Assert.Single(SysDirectory.GetDirectories(working));
        Assert.Contains($"'{kept}'", result.Errors[0].Message);
    }

    [Fact]
    public async Task Cancellation_during_restore_rolls_back_then_throws()
    {
        var routines = await BackedUpThenChangedAsync();
        var before = routines.Select(BackupServiceFixture.ReadLive).ToArray();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token);
        routines[1].DuringRestore = cts.Cancel;
        routines[1].OnRestore = RoutineBehavior.Cancel;
        var start = CallsSoFar;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RestoreAsync(cancellationToken: cts.Token));

        Assert.Equal(
            ["a:backup", "b:backup", "c:backup", "a:restore", "b:restore", "b:rollback", "a:rollback"],
            CallsSince(start)
        );
        Assert.Equal(before, routines.Select(BackupServiceFixture.ReadLive));
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Cancellation_after_a_successful_routine_rolls_it_back()
    {
        var routines = await BackedUpThenChangedAsync();
        var before = routines.Select(BackupServiceFixture.ReadLive).ToArray();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(Token);
        routines[0].DuringRestore = cts.Cancel;
        var start = CallsSoFar;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => RestoreAsync(cancellationToken: cts.Token));

        Assert.Equal(["a:backup", "b:backup", "c:backup", "a:restore", "a:rollback"], CallsSince(start));
        Assert.Equal(before, routines.Select(BackupServiceFixture.ReadLive));
    }

    [Fact]
    public async Task Unexpected_exception_after_a_routine_ran_still_rolls_back()
    {
        var routines = await BackedUpThenChangedAsync();
        var before = routines.Select(BackupServiceFixture.ReadLive).ToArray();
        var progress = new ThrowingProgress(new BackupProgress(BackupStage.Restoring, "b"));
        var start = CallsSoFar;

        await Assert.ThrowsAsync<InvalidOperationException>(() => RestoreAsync(progress: progress));

        Assert.Equal(["a:backup", "b:backup", "c:backup", "a:restore", "b:rollback", "a:rollback"], CallsSince(start));
        Assert.Equal(before, routines.Select(BackupServiceFixture.ReadLive));
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Progress_receiver_failing_during_rollback_does_not_stop_it()
    {
        var routines = await BackedUpThenChangedAsync();
        var before = routines.Select(BackupServiceFixture.ReadLive).ToArray();
        routines[1].OnRestore = RoutineBehavior.Fail;
        var progress = new ThrowingProgress(new BackupProgress(BackupStage.RollingBack, "b"));

        var result = await RestoreAsync(progress: progress);

        Assert.Equal("b failed", Assert.Single(result.Errors).Message);
        Assert.Equal(["b:rollback", "a:rollback"], _fixture.Calls.Calls.TakeLast(2));
        Assert.Equal(before, routines.Select(BackupServiceFixture.ReadLive));
    }

    private sealed class ThrowingProgress(BackupProgress trigger) : IProgress<BackupProgress>
    {
        public void Report(BackupProgress value)
        {
            if (value == trigger) { throw new InvalidOperationException("progress receiver failed"); }
        }
    }

    [Fact]
    public async Task Routine_cancelling_on_its_own_is_a_failure()
    {
        var routines = await BackedUpThenChangedAsync();
        routines[1].OnRestore = RoutineBehavior.Cancel;

        var result = await RestoreAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Failure, result.Errors[0].Type);
        Assert.Equal("a:rollback", _fixture.Calls.Calls[^1]);
    }

    // ---- Guards -----------------------------------------------------------

    [Fact]
    public async Task No_routines_is_a_validation_error()
    {
        var result = await RestoreAsync();

        Assert.False(result.Success);
        Assert.Equal(ErrorType.Validation, result.Errors[0].Type);
    }

    [Fact]
    public async Task Concurrent_operation_is_a_conflict()
    {
        _fixture.AddRoutine("a", files: ("a.txt", "a"));
        await _fixture.BackupAsync();
        using var held = _fixture.Gate.TryEnter();

        await AssertRejectedAsync(ErrorType.Conflict);
    }

    [Fact]
    public async Task Null_request_throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _fixture.CreateService().RestoreAsync(null!, null, Token));
    }
}
