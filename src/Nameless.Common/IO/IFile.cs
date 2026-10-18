using Nameless.IO.Monitoring;

namespace Nameless.IO;

/// <summary>
///     Defines a file in the file system.
/// </summary>
public interface IFile {
    /// <summary>
    ///     Gets the name of the file.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the path of the file.
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     Whether the file exists.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    ///     Gets the file length.
    /// </summary>
    long Length { get; }

    /// <summary>
    ///     Gets the last write time of the file in UTC.
    /// </summary>
    DateTimeOffset LastWriteTime { get; }

    /// <summary>
    ///     Opens the file with the specified mode, access, and
    ///     share options.
    /// </summary>
    /// <param name="mode">
    ///     The file mode to use when opening the file.
    /// </param>
    /// <param name="access">
    ///     The file access level to use when opening the file.
    /// </param>
    /// <param name="share">
    ///     The file share mode to use when opening the file.
    /// </param>
    /// <returns>
    ///     A <see cref="Stream"/> representing the opened file.
    /// </returns>
    Stream Open(FileMode mode, FileAccess access, FileShare share);

    /// <summary>
    ///     Deletes the file.
    /// </summary>
    void Delete();

    /// <summary>
    ///     Copies the file to the specified destination path.
    /// </summary>
    /// <param name="destinationRelativePath">
    ///     The relative path where the file should be copied.
    /// </param>
    /// <param name="overwrite">
    ///     Whether to overwrite the file if it already exists
    ///     at the destination.
    /// </param>
    /// <returns>
    ///     The copied <see cref="IFile"/>.
    /// </returns>
    IFile Copy(string destinationRelativePath, bool overwrite);

    /// <summary>
    ///     Creates a file monitor for this file.
    /// </summary>
    /// <param name="configure">
    ///     The delegate to configure the file monitor.
    /// </param>
    /// <returns>
    ///     The file monitor.
    /// </returns>
    IFileMonitor Monitor(Action<FileMonitorOptions>? configure = null);
}