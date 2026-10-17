using Nameless.Backup.Infrastructure;
using Nameless.Testing.Tools.IO;

namespace Nameless.Backup;

[UnitTest]
public sealed class BackupRoutineCatalogTests : IDisposable
{
    private readonly TempDirectory _temp = new();
    private readonly CallLog _log = new();

    public void Dispose()
    {
        _temp.Dispose();
    }

    private TestRoutine Routine(string key, int order = 0)
    {
        return new TestRoutine(key, order, _temp.Combine("live", Guid.NewGuid().ToString("N")), _log);
    }

    [Fact]
    public void Routines_are_sorted_by_order_then_key()
    {
        var catalog = new BackupRoutineCatalog([Routine("b", 1), Routine("c", 0), Routine("a", 1)]);

        Assert.Equal(["c", "a", "b"], catalog.Routines.Select(routine => routine.Key));
    }

    [Fact]
    public void TryGet_finds_routine_by_key()
    {
        var routine = Routine("db");
        var catalog = new BackupRoutineCatalog([routine]);

        Assert.True(catalog.TryGet("db", out var found));
        Assert.Same(routine, found);
        Assert.False(catalog.TryGet("other", out _));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("db")]
    [InlineData("app-data_v2.sqlite")]
    [InlineData("0")]
    public void Valid_keys_are_accepted(string key)
    {
        Assert.True(BackupRoutineCatalog.IsValidKey(key));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("DB")]
    [InlineData("..")]
    [InlineData("a/b")]
    [InlineData("a\\b")]
    [InlineData("-a")]
    [InlineData("a.")]
    [InlineData("a b")]
    [InlineData("con")]
    [InlineData("nul.db")]
    [InlineData("com1")]
    public void Invalid_keys_are_rejected(string? key)
    {
        Assert.False(BackupRoutineCatalog.IsValidKey(key));
    }

    [Fact]
    public void Key_longer_than_64_chars_is_rejected()
    {
        Assert.True(BackupRoutineCatalog.IsValidKey(new string('a', 64)));
        Assert.False(BackupRoutineCatalog.IsValidKey(new string('a', 65)));
    }

    [Fact]
    public void Invalid_key_throws_naming_the_routine_type()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new BackupRoutineCatalog([Routine("Bad Key")]));

        Assert.Contains(nameof(TestRoutine), exception.Message);
        Assert.Contains("Bad Key", exception.Message);
    }

    [Fact]
    public void Duplicate_key_throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => new BackupRoutineCatalog([Routine("db"), Routine("db")]));

        Assert.Contains("'db'", exception.Message);
    }
}
