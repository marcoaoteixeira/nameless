using System.IO.Compression;

namespace Nameless.Backup;

[UnitTest]
public class BackupContractsTests
{
    [Fact]
    public void BackupOptions_defaults()
    {
        var options = new BackupOptions();

        Assert.Equal(SysPath.Combine(SysPath.GetTempPath(), "Backup"), options.WorkingDirectoryPath);
        Assert.Equal(CompressionLevel.Optimal, options.CompressionLevel);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BackupOptions_rejects_missing_working_directory(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new BackupOptions { WorkingDirectoryPath = value! });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BackupRequest_rejects_missing_destination(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new BackupRequest { DestinationFilePath = value! });
    }

    [Fact]
    public void BackupRequest_does_not_overwrite_by_default()
    {
        var request = new BackupRequest { DestinationFilePath = "a.zip" };

        Assert.False(request.Overwrite);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RestoreRequest_rejects_missing_archive(string? value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new RestoreRequest { ArchiveFilePath = value! });
    }

    [Fact]
    public void RestoreRequest_has_no_checksum_by_default()
    {
        var request = new RestoreRequest { ArchiveFilePath = "a.zip" };

        Assert.Null(request.Checksum);
    }
}
