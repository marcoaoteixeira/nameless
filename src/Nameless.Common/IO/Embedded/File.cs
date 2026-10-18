using System.Diagnostics;
using Nameless.IO.Monitoring;

namespace Nameless.IO.Embedded;

/// <summary>
///     Read-only implementation of <see cref="IFile"/> for a file
///     embedded as resource into an assembly.
/// </summary>
[DebuggerDisplay(value: "{Path,nq}")]
public class File : IFile {
    private readonly ManifestFileInfoWrapper _file;

    /// <inheritdoc />
    public string Name => _file.Name;

    /// <inheritdoc />
    public string Path => _file.PhysicalPath ?? string.Empty;

    /// <inheritdoc />
    public bool Exists => _file.Exists;

    /// <inheritdoc />
    public long Length => _file.Length;

    /// <inheritdoc />
    public DateTimeOffset LastWriteTime => _file.LastModified;

    /// <summary>
    ///     Initializes a new instance of <see cref="File"/> class.
    /// </summary>
    /// <param name="file">
    ///     The inner embedded resource file info.
    /// </param>
    internal File(ManifestFileInfoWrapper file) {
        _file = file;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     <paramref name="mode"/>, <paramref name="access"/> and
    ///     <paramref name="share"/> are ignored since they do not apply
    ///     to embedded resources. The returned stream is read-only.
    /// </remarks>
    /// <exception cref="FileNotFoundException">
    ///     if the file is not embedded into the assembly.
    /// </exception>
    public Stream Open(FileMode mode, FileAccess access, FileShare share) {
        return _file.CreateReadStream();
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     always, embedded files cannot be deleted.
    /// </exception>
    public void Delete() {
        throw new InvalidOperationException("Embedded files cannot be deleted.");
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     always, embedded files cannot be copied into the assembly.
    /// </exception>
    public IFile Copy(string destinationRelativePath, bool overwrite) {
        throw new InvalidOperationException("Embedded files cannot be copied.");
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     always, embedded files cannot be monitored.
    /// </exception>
    public IFileMonitor Monitor(Action<FileMonitorOptions>? configure = null) {
        throw new InvalidOperationException("Embedded files cannot be monitored.");
    }
}