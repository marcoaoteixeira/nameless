using Moq;
using Nameless.Backup.Internals;
using Nameless.Compression;
using Nameless.ObjectModel;
using Nameless.Testing.Tools.IO;

namespace Nameless.Backup;

[UnitTest]
public sealed class BackupArchiveReaderTests : IDisposable
{
    private const string ARCHIVE = "app.backup";
    private const string DESTINATION = "extracted";

    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 11, 9, 30, 0, TimeSpan.Zero);

    private readonly Mock<ICompressor> _compressor = new();
    private readonly TempDirectory _temp = new();

    public void Dispose()
    {
        _temp.Dispose();
    }

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    private string PayloadPath => _temp.Combine(BackupManifest.PayloadDirectoryName);

    private void DecompressThrows(Exception exception)
    {
        _compressor.Setup(compressor => compressor.DecompressAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(exception);
    }

    private static Exception CreateException(string name)
    {
        return name switch
        {
            nameof(ChecksumMismatchException) => new ChecksumMismatchException(new string('0', 64), new string('1', 64)),
            nameof(InvalidDataException) => new InvalidDataException("not a zip"),
            nameof(FileNotFoundException) => new FileNotFoundException("gone", ARCHIVE),
            nameof(IOException) => new IOException("disk full"),
            nameof(UnauthorizedAccessException) => new UnauthorizedAccessException("denied"),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }

    /// <summary>
    ///     A payload folder with one routine and its manifest.
    /// </summary>
    private async Task WriteValidPayloadAsync()
    {
        _temp.CreateFile(SysPath.Combine(BackupManifest.PayloadDirectoryName, "db", "a.txt"), "hello");

        var routine = await BackupManifestBuilder.DescribeAsync("db", 0, SysPath.Combine(PayloadPath, "db"), Token);
        var manifest = new BackupManifest(BackupManifest.CurrentFormatVersion, CreatedAt, [routine.Value]);

        await BackupManifestSerializer.WriteAsync(manifest, SysPath.Combine(PayloadPath, BackupManifest.FileName), Token);
    }

    // ---- Extract ----------------------------------------------------------

    [Fact]
    public async Task Extract_decompresses_without_overwriting()
    {
        var checksum = new string('a', 64);
        _compressor.Setup(compressor => compressor.DecompressAsync(ARCHIVE, DESTINATION, checksum, false, Token))
                   .ReturnsAsync(DESTINATION);
        var sut = new BackupArchiveReader(_compressor.Object);

        var result = await sut.ExtractAsync(ARCHIVE, checksum, DESTINATION, Token);

        Assert.True(result.Success);
        _compressor.Verify(compressor => compressor.DecompressAsync(ARCHIVE, DESTINATION, checksum, false, Token), Times.Once);
    }

    [Theory]
    [InlineData(nameof(ChecksumMismatchException), ErrorType.Validation)]
    [InlineData(nameof(InvalidDataException), ErrorType.Validation)]
    [InlineData(nameof(FileNotFoundException), ErrorType.Missing)]
    [InlineData(nameof(IOException), ErrorType.Failure)]
    [InlineData(nameof(UnauthorizedAccessException), ErrorType.Failure)]
    public async Task Extract_maps_compressor_failures_to_errors(string exception, ErrorType expected)
    {
        var thrown = CreateException(exception);
        DecompressThrows(thrown);
        var sut = new BackupArchiveReader(_compressor.Object);

        var result = await sut.ExtractAsync(ARCHIVE, checksum: null, DESTINATION, Token);

        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        Assert.Equal(expected, error.Type);
        Assert.Contains(ARCHIVE, error.Message);
    }

    [Fact]
    public async Task Extract_lets_cancellation_surface()
    {
        DecompressThrows(new OperationCanceledException());
        var sut = new BackupArchiveReader(_compressor.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => sut.ExtractAsync(ARCHIVE, checksum: null, DESTINATION, Token));
    }

    [Fact]
    public async Task Extract_lets_programming_errors_surface()
    {
        // the checksum is validated before extraction, so an argument error
        // here is a bug, not bad input
        DecompressThrows(new ArgumentException("bad"));
        var sut = new BackupArchiveReader(_compressor.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.ExtractAsync(ARCHIVE, checksum: null, DESTINATION, Token));
    }

    [Fact]
    public void Constructor_rejects_null_compressor()
    {
        Assert.Throws<ArgumentNullException>(() => new BackupArchiveReader(null!));
    }

    // ---- Read manifest ----------------------------------------------------

    [Fact]
    public async Task Read_manifest_returns_the_verified_manifest()
    {
        await WriteValidPayloadAsync();

        var result = await BackupArchiveReader.ReadManifestAsync(_temp.Path, Token);

        Assert.True(result.Success, result.Success ? null : result.Errors[0].Flatten);
        Assert.Equal(CreatedAt, result.Value.CreatedAt);
        Assert.Equal("db", Assert.Single(result.Value.Routines).Key);
    }

    [Fact]
    public async Task Read_manifest_rejects_unexpected_top_level_entries()
    {
        await WriteValidPayloadAsync();
        _temp.CreateFile("other.txt", "other");
        _temp.CreateDirectory("more");

        var result = await BackupArchiveReader.ReadManifestAsync(_temp.Path, Token);

        var error = Assert.Single(result.Errors);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Contains("other.txt", error.Message);
        Assert.Contains("more", error.Message);
    }

    [Fact]
    public async Task Read_manifest_rejects_a_missing_manifest()
    {
        _temp.CreateFile(SysPath.Combine(BackupManifest.PayloadDirectoryName, "db", "a.txt"), "hello");

        var result = await BackupArchiveReader.ReadManifestAsync(_temp.Path, Token);

        Assert.Equal(ErrorType.Validation, Assert.Single(result.Errors).Type);
    }

    [Fact]
    public async Task Read_manifest_rejects_files_that_do_not_match()
    {
        await WriteValidPayloadAsync();
        _temp.CreateFile(SysPath.Combine(BackupManifest.PayloadDirectoryName, "db", "a.txt"), "tampered");

        var result = await BackupArchiveReader.ReadManifestAsync(_temp.Path, Token);

        Assert.False(result.Success);
        Assert.All(result.Errors, error => Assert.Equal(ErrorType.Validation, error.Type));
    }
}
