using System.Diagnostics;
using Nameless.Helpers;
using Nameless.IO.Monitoring;

namespace Nameless.IO.Physical;

/// <summary>
///     Default implementation of <see cref="IFile"/>.
/// </summary>
[DebuggerDisplay(value: "{RelativePath,nq}")]
public class File : IFile {
    private string RelativePath => SysPath.GetRelativePath(_provider.Root, _file.FullName);

    private readonly FileInfo _file;
    private readonly FileProvider _provider;

    /// <inheritdoc />
    public string Name => _file.Name;

    /// <inheritdoc />
    public string Path => _file.FullName;

    /// <inheritdoc />
    public bool Exists => _file.Exists;

    /// <inheritdoc />
    public long Length => _file.Length;

    /// <inheritdoc />
    public DateTimeOffset LastWriteTime => _file.LastWriteTimeUtc;

    /// <summary>
    ///     Initializes a new instance of
    ///     the <see cref="File"/> class.
    /// </summary>
    /// <param name="file">
    ///     The underlying <see cref="FileInfo"/> object.
    /// </param>
    /// <param name="provider">
    ///     The file provider.
    /// </param>
    public File(FileInfo file, FileProvider provider) {
        _file = file;
        _provider = provider;
    }

    /// <inheritdoc />
    public Stream Open(FileMode mode, FileAccess access, FileShare share) {
        return _file.Open(mode, access, share);
    }

    /// <inheritdoc />
    public void Delete() {
        _file.Delete();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     The caller must register handlers, call
    ///     <see cref="IFileMonitor.Start" /> and dispose it.
    /// </remarks>
    public IFileMonitor Monitor(Action<FileMonitorOptions>? configure = null) {
        var options = ActionHelper.FromDelegate(configure);

        return new FileMonitor(
            _provider.Root,
            Name,
            new FileSystemWatcherAdapter(),
            FileProbe.Instance,
            TimeProvider.System,
            options
        );
    }

    /// <inheritdoc />
    public IFile Copy(string destinationRelativePath, bool overwrite) {
        var path = _provider.GetFullPath(
            destinationRelativePath
        );

        var file = _file.CopyTo(path, overwrite);
        
        return new File(file, _provider);
    }
}