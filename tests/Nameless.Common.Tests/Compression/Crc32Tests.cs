using System.IO.Compression;
using Nameless.Compression.Internals;

namespace Nameless.Compression;

public sealed class Crc32Tests
{
    [Fact]
    public void Matches_Standard_Check_Value()
    {
        // CRC-32/ISO-HDLC check value for "123456789".
        Assert.Equal(0xCBF43926u, Crc32.Compute("123456789"u8));
    }

    [Fact]
    public void Empty_Input_Is_Zero()
    {
        Assert.Equal(0u, Crc32.Compute([]));
    }

    [Fact]
    public void Incremental_Updates_Equal_One_Shot()
    {
        var data = "The quick brown fox jumps over the lazy dog"u8.ToArray();

        var crc = Crc32.Initial;
        crc = Crc32.Update(crc, data.AsSpan(0, 10));
        crc = Crc32.Update(crc, data.AsSpan(10));

        Assert.Equal(Crc32.Compute(data), Crc32.Finish(crc));
        Assert.Equal(0x414FA339u, Crc32.Compute(data));
    }

    [Fact]
    public void Matches_Crc_Stored_By_ZipArchive()
    {
        var data = new byte[5000];
        new Random(7).NextBytes(data);
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        using (var entry = archive.CreateEntry("x").Open())
        {
            entry.Write(data);
        }

        using var read = new ZipArchive(new MemoryStream(buffer.ToArray()), ZipArchiveMode.Read);

        Assert.Equal(read.Entries[0].Crc32, Crc32.Compute(data));
    }
}
