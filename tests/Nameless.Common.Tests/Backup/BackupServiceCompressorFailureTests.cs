using System.IO.Compression;
using Moq;
using Nameless.Backup.Infrastructure;
using Nameless.Compression;
using Nameless.ObjectModel;

namespace Nameless.Backup;

/// <summary>
///     Compressor failures the real <see cref="ZipCompressor"/> cannot be
///     made to produce on demand (disk full, access denied, races).
/// </summary>
[IntegrationTest]
public sealed class BackupServiceCompressorFailureTests : IDisposable
{
    private readonly BackupServiceFixture _fixture = new();
    private readonly Mock<ICompressor> _compressor = new();

    public BackupServiceCompressorFailureTests()
    {
        _fixture.Compressor = _compressor.Object;
        _fixture.AddRoutine("a", files: ("a.txt", "a-v1"));
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private void CompressThrows(Exception exception)
    {
        _compressor.Setup(compressor => compressor.CompressDirectoryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CompressionLevel>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(exception);
    }

    private void DecompressThrows(Exception exception)
    {
        _compressor.Setup(compressor => compressor.DecompressAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(exception);
    }

    private Task<BackupResult> BackupAsync(CancellationToken? cancellationToken = null)
    {
        return _fixture.CreateService().BackupAsync(
            new BackupRequest { DestinationFilePath = _fixture.ArchivePath },
            progress: null,
            cancellationToken ?? Token
        );
    }

    private Task<RestoreResult> RestoreAsync()
    {
        _fixture.Temp.CreateFile(SysPath.Combine("out", "app.backup"), "archive");

        return _fixture.CreateService().RestoreAsync(
            new RestoreRequest { ArchiveFilePath = _fixture.ArchivePath },
            progress: null,
            Token
        );
    }

    // ---- Backup -----------------------------------------------------------

    [Theory]
    [InlineData(typeof(IOException))]
    [InlineData(typeof(UnauthorizedAccessException))]
    public async Task Compression_failure_is_a_failure_and_staging_is_deleted(Type exception)
    {
        CompressThrows((Exception)Activator.CreateInstance(exception, "disk full")!);

        var result = await BackupAsync();

        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Contains("disk full", error.Message);
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    [Fact]
    public async Task Cancellation_during_compression_surfaces_and_staging_is_deleted()
    {
        using var cts = new CancellationTokenSource();
        _compressor.Setup(compressor => compressor.CompressDirectoryAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<CompressionLevel>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                   .Returns(async () =>
                   {
                       await cts.CancelAsync();
                       cts.Token.ThrowIfCancellationRequested();

                       return null!;
                   });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BackupAsync(cts.Token));

        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }

    // ---- Restore ----------------------------------------------------------

    [Theory]
    [InlineData(typeof(FileNotFoundException), ErrorType.Missing)]
    [InlineData(typeof(IOException), ErrorType.Failure)]
    [InlineData(typeof(UnauthorizedAccessException), ErrorType.Failure)]
    [InlineData(typeof(InvalidDataException), ErrorType.Validation)]
    public async Task Extraction_failure_is_an_error_and_nothing_runs(Type exception, ErrorType expected)
    {
        // FileNotFoundException: the archive vanished after the existence check
        DecompressThrows((Exception)Activator.CreateInstance(exception, "extraction failed")!);

        var result = await RestoreAsync();

        Assert.False(result.Success);
        Assert.Equal(expected, Assert.Single(result.Errors).Type);
        Assert.Empty(_fixture.Calls.Calls);
        Assert.Empty(_fixture.WorkingDirectoryEntries());
    }
}
