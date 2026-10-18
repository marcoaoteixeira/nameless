using Nameless.IO.Monitoring;

namespace Nameless.IO;

/// <summary>
///     Defines a directory in the file system.
/// </summary>
public interface IDirectory {
    /// <summary>
    ///     Gets the name of the directory.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the full path of the directory.
    /// </summary>
    string Path { get; }

    /// <summary>
    ///     Whether the directory exists.
    /// </summary>
    bool Exists { get; }

    /// <summary>
    ///     Creates the directory if it does not exist.
    /// </summary>
    void Create();

    /// <summary>
    ///     Retrieves files in the directory matching the
    ///     specified search pattern.
    /// </summary>
    /// <param name="glob">
    ///     The GLOB pattern to match files.
    /// </param>
    /// <returns>
    ///     A collection of <see cref="IFile"/> objects
    /// </returns>
    IEnumerable<IFile> GetFiles(string glob);

    /// <summary>
    ///     Deletes the directory.
    /// </summary>
    /// <param name="recursive">
    ///     Whether to delete subdirectories and files
    /// </param>
    void Delete(bool recursive);

    /// <summary>
    ///     Creates a file monitor that watches over the defined
    ///     glob pattern.
    /// </summary>
    /// <param name="glob">
    ///     The GLOB pattern.
    /// </param>
    /// <param name="configure">
    ///     The configuration delegate for the file monitor.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="IFileMonitor"/>.
    /// </returns>
    IFileMonitor Monitor(string? glob = null, Action<FileMonitorOptions>? configure = null);
}