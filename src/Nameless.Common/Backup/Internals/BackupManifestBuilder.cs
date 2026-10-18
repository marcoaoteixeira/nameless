using System.Security.Cryptography;
using Nameless;
using Nameless.ObjectModel;

namespace Nameless.Backup.Internals;

/// <summary>
///     Describes the files a routine wrote into its working directory.
/// </summary>
internal static class BackupManifestBuilder {
    // 80 KiB: the Stream.CopyTo default, the largest multiple of 4 KiB
    // that stays below the 85,000-byte Large Object Heap threshold
    private const int BUFFER_SIZE = 81_920;

    /// <summary>
    ///     Lists every file under <paramref name="directoryPath"/> with its
    ///     length and SHA-256.
    /// </summary>
    /// <returns>
    ///     The routine description, or an <see cref="ErrorType.Failure"/>
    ///     error if the directory holds a symbolic link or junction (the
    ///     archive would silently drop it).
    /// </returns>
    public static async Task<BackupManifestRoutineResult> DescribeAsync(string key, int order, string directoryPath, CancellationToken cancellationToken) {
        var files = new List<BackupManifestFile>();
        var pending = new Stack<DirectoryInfo>([new DirectoryInfo(directoryPath)]);

        while (pending.TryPop(out var directory)) {
            foreach (var entry in directory.EnumerateFileSystemInfos()) {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = ToRelativePath(directoryPath, entry.FullName);

                if (entry.LinkTarget is not null || entry.Attributes.HasFlag(FileAttributes.ReparsePoint)) {
                    return Error.Failure($"Backup routine '{key}' left the link '{relativePath}' in its working directory; links are not supported.");
                }

                if (entry is DirectoryInfo subdirectory) {
                    pending.Push(subdirectory);

                    continue;
                }

                var (length, sha256) = await HashFileAsync(
                    entry.FullName,
                    cancellationToken
                ).SkipContextSync();

                files.Add(new BackupManifestFile(relativePath, length, sha256));
            }
        }

        return new BackupManifestRoutine(
            Key: key,
            Order: order,
            Files: [.. files.OrderBy(file => file.Path, StringComparer.Ordinal)]
        );
    }

    internal static async Task<(long Length, string Sha256)> HashFileAsync(string filePath, CancellationToken cancellationToken) {
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, useAsync: true);

        await using (stream.ConfigureAwait(false)) {
            var hash = await SHA256.HashDataAsync(
                stream,
                cancellationToken
            ).ConfigureAwait(false);

            return (stream.Length, Convert.ToHexStringLower(hash));
        }
    }

    internal static string ToRelativePath(string root, string fullPath) {
        return SysPath.GetRelativePath(root, fullPath)
                      .Replace(SysPath.DirectorySeparatorChar, '/');
    }
}
