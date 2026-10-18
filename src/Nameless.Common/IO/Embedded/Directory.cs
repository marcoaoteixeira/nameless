using System.Diagnostics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Nameless.IO.Monitoring;

namespace Nameless.IO.Embedded;

/// <summary>
///     Read-only implementation of <see cref="IDirectory"/> for a directory
///     of files embedded as resources into an assembly.
/// </summary>
[DebuggerDisplay(value: "{Path,nq}")]
public class Directory : IDirectory {
    private readonly ManifestDirectoryInfoWrapper _directory;
    private readonly FileProvider _provider;

    /// <inheritdoc />
    public string Name => _directory.Name;

    /// <inheritdoc />
    public string Path => _directory.PhysicalPath;

    /// <inheritdoc />
    public bool Exists => _directory.Exists;

    internal Directory(ManifestDirectoryInfoWrapper directory, FileProvider provider) {
        _directory = directory;
        _provider = provider;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     always, directories cannot be created into the assembly.
    /// </exception>
    public void Create() {
        throw new InvalidOperationException("Embedded directories cannot be created.");
    }

    /// <inheritdoc />
    /// <remarks>
    ///     The match is case-insensitive, same as the embedded
    ///     files manifest lookups.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     if <paramref name="glob"/> is empty or white space.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="glob"/> is <see langword="null"/>.
    /// </exception>
    public IEnumerable<IFile> GetFiles(string glob) {
        Throws.When.NullOrWhiteSpace(glob);

        var recursive = glob.Contains("**") ||
                        glob.Contains('/') ||
                        glob.Contains('\\');

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase).AddInclude(glob);
        var entries = EnumerateFiles(string.Empty, _directory, recursive);
        var result = matcher.Match(entries);

        foreach (var match in result.Files) {
            yield return _provider.GetFile($"{_directory.RelativePath}/{match.Path}");
        }
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     always, embedded directories cannot be deleted.
    /// </exception>
    public void Delete(bool recursive) {
        throw new InvalidOperationException("Embedded directories cannot be deleted.");
    }

    /// <inhertidoc />
    public IFileMonitor Monitor(string? glob = null, Action<FileMonitorOptions>? configure = null) {
        throw new InvalidOperationException("Embedded directories cannot be monitored.");
    }

    // Returns the paths of all files underneath the directory,
    // relative to this directory.
    private static IEnumerable<string> EnumerateFiles(string relativePath, IDirectoryContents root, bool recursive) {
        foreach (var entry in root) {
            var entryRelativePath = relativePath.Length > 0
                ? $"{relativePath.TrimEnd('/')}/{entry.Name}"
                : entry.Name;

            if (entry.IsDirectory) {
                if (!recursive) { continue; }

                foreach (var subEntryRelativePath in  EnumerateFiles(entryRelativePath, (IDirectoryContents)entry, recursive)) {
                    yield return subEntryRelativePath;
                }
            }
            else { yield return entryRelativePath; }
        }
    }
}