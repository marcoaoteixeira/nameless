using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Backup.Infrastructure;
using Nameless.Compression;
using Nameless.Testing.Tools.IO;

namespace Nameless.Backup;

[IntegrationTest]
public sealed class BackupRoundTripTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose()
    {
        _temp.Dispose();
    }

    [Fact]
    public async Task Backup_then_restore_through_dependency_injection()
    {
        var calls = new CallLog();
        var settings = new TestRoutine("settings", 1, _temp.Combine("live", "settings"), calls);
        var database = new TestRoutine("database", 0, _temp.Combine("live", "database"), calls);
        BackupServiceFixture.WriteLive(settings, "app.json", "{ \"theme\": \"dark\" }");
        BackupServiceFixture.WriteLive(database, "app.db", "rows-v1");
        BackupServiceFixture.WriteLive(database, "blobs/1.bin", "blob");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Backup:WorkingDirectoryPath"] = _temp.Combine("work") })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.RegisterZipCompressor();
        services.AddSingleton<IBackupRoutine>(settings);
        services.AddSingleton<IBackupRoutine>(database);
        services.RegisterBackupService(registration => registration.WithUseAssemblyScan(false), configuration);

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IBackupService>();
        var token = TestContext.Current.CancellationToken;

        var backup = await service.BackupAsync(new BackupRequest { DestinationFilePath = _temp.Combine("out", "app.backup") }, null, token);

        Assert.True(backup.Success);
        Assert.Equal(["database", "settings"], backup.Value.RoutineKeys);
        Assert.Equal(ChecksumAlgorithm.Sha256, backup.Value.Algorithm);

        var original = new[] { BackupServiceFixture.ReadLive(database), BackupServiceFixture.ReadLive(settings) };
        BackupServiceFixture.WriteLive(database, "app.db", "rows-v2");
        SysFile.Delete(SysPath.Combine(database.LiveDirectoryPath, "blobs", "1.bin"));
        BackupServiceFixture.WriteLive(settings, "app.json", "{}");

        var restore = await service.RestoreAsync(new RestoreRequest { ArchiveFilePath = backup.Value.FilePath, Checksum = backup.Value.Checksum }, null, token);

        Assert.True(restore.Success, restore.Success ? null : restore.Errors[0].Flatten);
        Assert.Equal(original, new[] { BackupServiceFixture.ReadLive(database), BackupServiceFixture.ReadLive(settings) });
        Assert.Empty(SysDirectory.GetFileSystemEntries(_temp.Combine("work")));
    }
}
