using Microsoft.Extensions.Time.Testing;

namespace Nameless.IO.Monitoring;

[IntegrationTest]
public class FileSystemWatcherAdapterTests : IDisposable {
    private readonly string _root = SysPath.Combine(SysPath.GetTempPath(), $"watcher-{Guid.NewGuid():N}");

    public FileSystemWatcherAdapterTests() {
        SysDirectory.CreateDirectory(_root);
    }

    public void Dispose() {
        SysDirectory.Delete(_root, recursive: true);
    }

    [Fact]
    public void Properties_RoundTripToInnerWatcher() {
        // arrange
        using var sut = new FileSystemWatcherAdapter();

        // act
        sut.Path = _root;
        sut.IncludeSubdirectories = true;
        sut.InternalBufferSize = 16 * 1024;
        sut.EnableRaisingEvents = true;

        // assert
        Assert.Multiple(
            () => Assert.Equal(_root, sut.Path),
            () => Assert.True(sut.IncludeSubdirectories),
            () => Assert.Equal(16 * 1024, sut.InternalBufferSize),
            () => Assert.True(sut.EnableRaisingEvents)
        );
    }

    [Fact]
    public async Task Created_RaisedWhenFileIsCreated() {
        // arrange
        using var sut = new FileSystemWatcherAdapter { Path = _root };
        var created = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        sut.Created += (_, e) => created.TrySetResult(e.Name!);
        sut.Changed += (_, _) => { };
        sut.Deleted += (_, _) => { };
        sut.Renamed += (_, _) => { };
        sut.Error += (_, _) => { };
        sut.EnableRaisingEvents = true;

        // act
        SysFile.WriteAllText(SysPath.Combine(_root, "new.txt"), "x");

        // assert
        var winner = await Task.WhenAny(created.Task, Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));
        Assert.Same(created.Task, winner);
        Assert.Equal("new.txt", await created.Task);
    }

    [Fact]
    public void Events_CanBeUnsubscribed() {
        // arrange
        using var sut = new FileSystemWatcherAdapter { Path = _root };

        FileSystemEventHandler handler = (_, _) => { };
        RenamedEventHandler renamed = (_, _) => { };
        ErrorEventHandler error = (_, _) => { };

        // act
        var exception = Record.Exception(() => {
            sut.Created += handler;
            sut.Created -= handler;
            sut.Changed += handler;
            sut.Changed -= handler;
            sut.Deleted += handler;
            sut.Deleted -= handler;
            sut.Renamed += renamed;
            sut.Renamed -= renamed;
            sut.Error += error;
            sut.Error -= error;
        });

        // assert
        Assert.Null(exception);
    }
}