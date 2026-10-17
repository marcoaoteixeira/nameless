using Microsoft.Extensions.Time.Testing;
using Nameless.Backup.Internals;
using Nameless.Compression;
using Nameless.Compression.Infrastructure;
using Nameless.Testing.Tools.IO;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Nameless.Backup.Infrastructure;

/// <summary>
///     Builds a <see cref="BackupService"/> over temp directories, test
///     routines and (by default) the real <see cref="ZipCompressor"/>.
/// </summary>
internal sealed class BackupServiceFixture : IDisposable {
    public static readonly DateTimeOffset Now = new(2026, 10, 11, 10, 0, 0, TimeSpan.Zero);

    public TempDirectory Temp { get; } = new();

    public CallLog Calls { get; } = new();

    public FakeTimeProvider Time { get; } = new(Now);

    public ListLogger<BackupService> Logger { get; } = new();

    public BackupOperationGate Gate { get; } = new();

    public BackupOptions Options { get; }

    public ICompressor Compressor { get; set; } = ZipCompressorSut.Create();

    public List<TestRoutine> Routines { get; } = [];

    public string ArchivePath => Temp.Combine("out", "app.backup");

    public BackupServiceFixture() {
        Options = new BackupOptions { WorkingDirectoryPath = Temp.Combine("work") };
    }

    public void Dispose() {
        Gate.Dispose();
        Temp.Dispose();
    }

    public TestRoutine AddRoutine(string key, int order = 0, params (string Path, string Content)[] files) {
        var routine = new TestRoutine(key, order, Temp.Combine("live", key), Calls);

        foreach (var (path, content) in files) {
            Temp.CreateFile(SysPath.Combine("live", key, path), content);
        }

        Routines.Add(routine);

        return routine;
    }

    public BackupService CreateService() {
        return CreateService([.. Routines]);
    }

    public BackupService CreateService(params IBackupRoutine[] routines) {
        return new BackupService(
            routines,
            Compressor,
            Gate,
            Time,
            MsOptions.Create(Options), Logger);
    }

    public async Task<BackupMetadata> BackupAsync(BackupService? service = null) {
        var result = await (service ?? CreateService()).BackupAsync(
            new BackupRequest { DestinationFilePath = ArchivePath },
            progress: null,
            TestContext.Current.CancellationToken
        );

        Assert.True(result.Success, result.Success ? null : result.Errors[0].Flatten);

        return result.Value;
    }

    /// <summary>
    ///     Every file of the routine's live directory: relative path →
    ///     content.
    /// </summary>
    public static Dictionary<string, string> ReadLive(TestRoutine routine) {
        return SysDirectory.EnumerateFiles(routine.LiveDirectoryPath, "*", SearchOption.AllDirectories)
                           .ToDictionary(
                               path => SysPath.GetRelativePath(routine.LiveDirectoryPath, path).Replace('\\', '/'),
                               SysFile.ReadAllText
                           );
    }

    public static void WriteLive(TestRoutine routine, string path, string content) {
        var fullPath = SysPath.Combine(routine.LiveDirectoryPath, path);

        SysDirectory.CreateDirectory(SysPath.GetDirectoryName(fullPath)!);
        SysFile.WriteAllText(fullPath, content);
    }

    /// <summary>
    ///     Entries left in the working directory (staging must be deleted).
    /// </summary>
    public string[] WorkingDirectoryEntries() {
        return SysDirectory.Exists(Options.WorkingDirectoryPath)
            ? SysDirectory.GetFileSystemEntries(Options.WorkingDirectoryPath)
            : [];
    }
}

/// <summary>
///     Synchronous <see cref="IProgress{T}"/> recorder
///     (<see cref="Progress{T}"/> posts asynchronously).
/// </summary>
internal sealed class ProgressRecorder : IProgress<BackupProgress> {
    private readonly List<BackupProgress> _reports = [];

    public IReadOnlyList<BackupProgress> Reports => _reports;

    public void Report(BackupProgress value) {
        _reports.Add(value);
    }
}
