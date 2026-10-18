using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing;
using Nameless.Helpers;
using Nameless.IO.Monitoring;

namespace Nameless.IO.Physical;

/// <summary>
///     Default implementation of <see cref="IDirectory"/>.
/// </summary>
[DebuggerDisplay(value: "{RelativePath,nq}")]
public class Directory : IDirectory {
    private string RelativePath => SysPath.GetRelativePath(_provider.Root, _directory.FullName);

    private static readonly StringComparison MatcherComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private readonly DirectoryInfo _directory;
    private readonly FileProvider _provider;

    /// <inheritdoc />
    public string Name => _directory.Name;

    /// <inheritdoc />
    public string Path => _directory.FullName;

    /// <inheritdoc />
    public bool Exists => _directory.Exists;

    /// <summary>
    ///     Initializes a new instance of
    ///     the <see cref="Directory"/> class.
    /// </summary>
    /// <param name="directory">
    ///     The underlying <see cref="DirectoryInfo"/> object.
    /// </param>
    /// <param name="provider">
    ///     The options for configuring the file system.
    /// </param>
    public Directory(DirectoryInfo directory, FileProvider provider) {
        _directory = directory;
        _provider = provider;
    }

    /// <inheritdoc />
    public void Create() {
        _directory.Create();
    }
    
    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     if <paramref name="glob"/> is empty or white space.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="glob"/> is <see langword="null"/>.
    /// </exception>
    public IEnumerable<IFile> GetFiles(string glob) {
        Throws.When.NullOrWhiteSpace(glob);

        var matcher = new Matcher(MatcherComparison).AddInclude(glob);
        
        foreach (var file in matcher.GetResultsInFullPath(Path)) {
            yield return _provider.GetFile(
                SysPath.GetRelativePath(_provider.Root, file)
            );
        }
    }

    /// <inheritdoc />
    public void Delete(bool recursive) {
        _directory.Delete(recursive);
    }

    /// <inheritdoc />
    public IFileMonitor Monitor(string? glob = null, Action<FileMonitorOptions>? configure = null) {
        var options = ActionHelper.FromDelegate(configure);

        return new FileMonitor(
            _provider.Root,
            glob ?? "**/*",
            new FileSystemWatcherAdapter(),
            FileProbe.Instance,
            TimeProvider.System,
            options
        );
    }
}