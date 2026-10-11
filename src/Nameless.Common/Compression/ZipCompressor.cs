using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nameless.Compression.Internals;
using Nameless.IO;

namespace Nameless.Compression;

/// <summary>
///     <see cref="ICompressor"/> implementation backed by
///     <see cref="ZipArchive"/>.
/// </summary>
/// <remarks>
///     Arguments are validated eagerly (exceptions surface when the method
///     is called); file system checks and I/O happen inside the returned
///     task. Archives are written to a temporary file next to the destination
///     and moved into place only when complete, so a failed or cancelled
///     operation never leaves a partial archive. Extraction is delegated to
///     <see cref="ZipExtractor"/>, which enforces the configured
///     <see cref="ZipCompressorOptions"/>.
/// </remarks>
public sealed class ZipCompressor : ICompressor {
    private const string PARENT_SEGMENT = "..";
    private const int BUFFER_SIZE = 81_920;

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private readonly ArchiveChecksum _checksum;
    private readonly ZipExtractor _extractor;
    private readonly ILogger _logger;

    /// <summary>
    ///     Initializes a new instance of <see cref="ZipCompressor"/> class. 
    /// </summary>
    /// <remarks>
    ///     Every parameter is optional, which also lets dependency injection
    ///     containers fall back to the defaults for whatever is not
    ///     registered.
    /// </remarks>
    /// <param name="options">
    ///     The Zip Compressor settings.
    /// </param>
    /// <param name="logger">
    ///     Diagnostics sink. Keys and checksums are never logged.
    /// </param>
    public ZipCompressor(IOptions<ZipCompressorOptions> options, ILogger<ZipCompressor> logger) {
        Throws.When.Null(options);
        Throws.When.Null(options.Value, paramName: nameof(options));

        _logger = Throws.When.Null(logger);

        _checksum = new ArchiveChecksum(options.Value.HmacKey);
        _extractor = new ZipExtractor(options.Value, logger);
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Possibly spread over several drives and network shares. Each
    ///     entry is the file's full path minus its root (
    ///     <c>C:\work\a.txt</c> and <c>\\server\share\work\b.txt</c>
    ///     → <c>work/a.txt</c>, <c>work/b.txt</c>). Duplicate paths are
    ///     included once.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="filePaths"/> or
    ///     <paramref name="destinationFilePath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="filePaths"/> is empty, contains a null, empty or
    ///     white space path, or two different files would be stored under
    ///     the same entry name (e.g. <c>C:\a\x.txt</c> and <c>D:\a\x.txt</c>);
    ///     or <paramref name="destinationFilePath"/> is empty or white space.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///     A path does not lead to an existing file.
    /// </exception>
    /// <exception cref="IOException">
    ///     The archive exists and <paramref name="overwrite"/> is
    ///     <see langword="false"/>.
    /// </exception>
    public Task<CompressionResult> CompressFilesAsync(IEnumerable<string> filePaths, string destinationFilePath, CompressionLevel compressionLevel, bool overwrite, CancellationToken cancellationToken) {
        Throws.When.Null(filePaths);

        string[] paths = [.. filePaths];

        Throws.When.Empty(paths, paramName: nameof(filePaths), message: "At least one file path is required.");
        Throws.When.AnyNullOrWhiteSpace(paths, paramName: nameof(filePaths), message: "File paths cannot be null, empty or white space.");
        Throws.When.NullOrWhiteSpace(destinationFilePath);
        Throws.When.UnknownEnum(compressionLevel, message: "Unknown compression level.");

        return CompressFilesAsyncCore(
            paths,
            destinationFilePath,
            compressionLevel,
            overwrite,
            cancellationToken
        );
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="directoryPath"/> or
    ///     <paramref name="destinationFilePath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="directoryPath"/>, <paramref name="globPattern"/>
    ///     or <paramref name="destinationFilePath"/> is empty or white
    ///     space; or <paramref name="globPattern"/> is rooted or contains a
    ///     <c>..</c> segment.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    ///     <paramref name="directoryPath"/> does not exist.
    /// </exception>
    /// <exception cref="IOException">
    ///     The archive exists and <paramref name="overwrite"/> is
    ///     <see langword="false"/>.
    /// </exception>
    public Task<CompressionResult> CompressDirectoryAsync(string directoryPath, string? globPattern, string destinationFilePath, CompressionLevel compressionLevel, bool overwrite, CancellationToken cancellationToken) {
        Throws.When.NullOrWhiteSpace(directoryPath);

        if (globPattern is not null) {
            ValidateGlobPattern(globPattern);
        }

        Throws.When.NullOrWhiteSpace(destinationFilePath);
        Throws.When.UnknownEnum(compressionLevel, message: "Unknown compression level.");

        return CompressDirectoryAsyncCore(
            directoryPath,
            globPattern,
            destinationFilePath,
            compressionLevel,
            overwrite,
            cancellationToken
        );
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="filePath"/> or
    ///     <paramref name="destinationDirectoryPath"/> is
    ///     <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="filePath"/> or
    ///     <paramref name="destinationDirectoryPath"/> is empty or white
    ///     space; or <paramref name="checksum"/> is not a 64-character hex
    ///     string.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    ///     <paramref name="filePath"/> does not exist.
    /// </exception>
    /// <exception cref="ChecksumMismatchException">
    ///     The archive does not match <paramref name="checksum"/>. Nothing
    ///     is extracted.
    /// </exception>
    /// <exception cref="InvalidDataException">
    ///     The file is not a valid ZIP archive, or it exceeds the
    ///     <see cref="ZipCompressorOptions"/>.
    /// </exception>
    /// <exception cref="IOException">
    ///     An entry is unsafe (outside the destination, or a reserved device
    ///     name on Windows) or a target file exists and
    ///     <paramref name="overwrite"/> is <see langword="false"/> — both
    ///     detected before anything is written; or a write fails (disk full,
    ///     file locked).
    /// </exception>
    public Task<string> DecompressAsync(string filePath, string destinationDirectoryPath, string? checksum, bool overwrite, CancellationToken cancellationToken) {
        Throws.When.NullOrWhiteSpace(filePath);
        Throws.When.NullOrWhiteSpace(destinationDirectoryPath);

        ArchiveChecksum.Validate(checksum);

        return DecompressAsyncCore(
            filePath,
            destinationDirectoryPath,
            checksum,
            overwrite,
            cancellationToken
        );
    }

    private async Task<CompressionResult> CompressFilesAsyncCore(string[] paths, string destinationFilePath, CompressionLevel compressionLevel, bool overwrite, CancellationToken cancellationToken) {
        var seen = new HashSet<string>(StringComparer.FromComparison(PathComparison));
        var files = new List<string>(paths.Length);

        foreach (var path in paths) {
            var fullPath = SysPath.GetFullPath(path);

            if (!SysFile.Exists(fullPath)) {
                throw new FileNotFoundException($"No file was found at '{path}'.", path);
            }

            if (seen.Add(fullPath)) {
                files.Add(fullPath);
            }
        }

        var items = FileListLayout.Create(files, PathComparison)
            .Select(entry => ArchiveItem.ForFile(entry.EntryName, entry.SourcePath))
            .ToArray();

        return await WriteArchiveAsync(
            source: $"{files.Count} listed files",
            destination: SysPath.GetFullPath(destinationFilePath),
            overwrite: overwrite,
            items: items,
            compressionLevel: compressionLevel,
            cancellationToken: cancellationToken
        ).SkipContextSync();
    }

    private async Task<CompressionResult> CompressDirectoryAsyncCore(string directoryPath, string? globPattern, string destinationFilePath, CompressionLevel compressionLevel, bool overwrite, CancellationToken cancellationToken) {
        var root = SysPath.TrimEndingDirectorySeparator(
            SysPath.GetFullPath(directoryPath)
        );

        if (!SysDirectory.Exists(root)) {
            throw new DirectoryNotFoundException(
                $"No directory was found at '{directoryPath}'."
            );
        }

        var destination = SysPath.GetFullPath(destinationFilePath);
        var snapshot = TakeSnapshot(root, cancellationToken);
        var items = globPattern is null
            ? DirectoryItems(root, snapshot, excludedFile: destination)
            : GlobItems(root, snapshot, globPattern, excludedFile: destination);

        return await WriteArchiveAsync(
            root,
            destination,
            overwrite,
            items,
            compressionLevel,
            cancellationToken
        ).SkipContextSync();
    }

    private async Task<string> DecompressAsyncCore(string filePath, string destinationDirectoryPath, string? checksum, bool overwrite, CancellationToken cancellationToken) {
        var fullPath = SysPath.GetFullPath(filePath);

        if (!SysFile.Exists(fullPath)) {
            throw new FileNotFoundException($"No archive was found at '{filePath}'.", filePath);
        }

        var destination = SysPath.GetFullPath(destinationDirectoryPath);

        // A single handle that denies writers: the bytes verified are the
        // bytes extracted.
        await using var source = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BUFFER_SIZE,
            FileOptions.Asynchronous
        );

        if (checksum is not null) {
            var actual = await _checksum.ComputeAsync(source, cancellationToken)
                                        .SkipContextSync();

            EnsureChecksum(checksum, actual, fullPath);

            source.Position = 0;
        }

        await _extractor.ExtractAsync(source, destination, overwrite, cancellationToken)
                        .SkipContextSync();

        return destination;
    }

    private void EnsureChecksum(string expected, byte[] actual, string source) {
        if (ArchiveChecksum.Matches(expected, actual)) {
            return;
        }

        Log.ChecksumMismatch(_logger, _checksum.Algorithm, source);

        throw _checksum.Mismatch(expected, actual);
    }

    private async Task<CompressionResult> WriteArchiveAsync(string source, string destination, bool overwrite, IReadOnlyList<ArchiveItem> items, CompressionLevel compressionLevel, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        if (!overwrite && SysFile.Exists(destination)) {
            throw new IOException($"The archive '{destination}' already exists.");
        }

        Log.CompressionStarted(_logger, source, destination, compressionLevel);

        var started = Stopwatch.GetTimestamp();
        var directory = SysPath.GetDirectoryName(destination)!;

        SysDirectory.CreateDirectory(directory);

        var temporary = SysPath.Combine(
            directory,
            $".{SysPath.GetFileName(destination)}.{Guid.NewGuid():N}.tmp"
        );

        try {
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, BUFFER_SIZE, FileOptions.Asynchronous))
            await using (var archive = await ZipArchive.CreateAsync(output, ZipArchiveMode.Create, leaveOpen: false, entryNameEncoding: null, cancellationToken).SkipContextSync()) {
                foreach (var item in items) {
                    cancellationToken.ThrowIfCancellationRequested();

                    Log.EntryAdded(_logger, item.EntryName, item.SourcePath ?? "(directory)");

                    if (item.SourcePath is null) {
                        archive.CreateEntry(item.EntryName);

                        continue;
                    }

                    await archive.CreateEntryFromFileAsync(item.SourcePath, item.EntryName, compressionLevel, cancellationToken)
                                 .SkipContextSync();
                }
            }

            // Hashed after the archive is closed: ZipArchive seeks back to
            // patch headers while writing.
            byte[] hash;
            long length;

            await using (var written = new FileStream(temporary, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, FileOptions.Asynchronous)) {
                length = written.Length;

                hash = await _checksum.ComputeAsync(written, cancellationToken)
                                      .SkipContextSync();
            }

            SysFile.Move(temporary, destination, overwrite);

            Log.CompressionCompleted(
                _logger,
                destination,
                items.Count,
                length,
                _checksum.Algorithm,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds
            );

            return new CompressionResult(
                destination,
                ArchiveChecksum.ToHex(hash),
                _checksum.Algorithm
            );
        }
        catch (Exception ex) {
            TryDelete(temporary);
            
            Log.CompressionFailed(_logger, ex, source);
            
            throw;
        }
    }

    private DirectorySnapshot TakeSnapshot(string root, CancellationToken cancellationToken) {
        var snapshot = DirectorySnapshot.Take(root, cancellationToken);

        foreach (var link in snapshot.SkippedLinks) {
            Log.LinkSkipped(_logger, link, root);
        }

        return snapshot;
    }

    /// <summary>
    ///     Every file plus an explicit entry for each empty directory, all
    ///     under the root's base folder name.
    /// </summary>
    private static ArchiveItem[] DirectoryItems(string root, DirectorySnapshot snapshot, string excludedFile) {
        var prefix = EntryPrefixFor(root);

        var files = snapshot.Files
                            .Where(file => !IsSamePath(file, excludedFile))
                            .Select(file => ArchiveItem.ForFile(
                                entryName: $"{prefix}{ToEntryPath(root, file)}",
                                sourcePath: file
                            ));

        var directories = snapshot.EmptyDirectories.Select(directory => ArchiveItem.ForDirectory(
            IsSamePath(SysPath.TrimEndingDirectorySeparator(directory), root)
                ? prefix
                : $"{prefix}{ToEntryPath(root, directory)}{Separators.ForwardSlash}"
        ));

        var items = files.Concat(directories)
                         .Where(item => item.EntryName.Length > 0)
                         .OrderBy(item => item.EntryName, StringComparer.Ordinal)
                         .ToArray();

        // A directory holding only the archive being written still deserves
        // its root entry.
        return items.Length == 0 && prefix.Length > 0
            ? [ArchiveItem.ForDirectory(prefix)]
            : items;
    }

    /// <summary>
    ///     Matching files only, under the root's base folder name. Matching
    ///     runs in memory over the link-free snapshot, so the glob can never
    ///     reach links or leave the root.
    /// </summary>
    private ArchiveItem[] GlobItems(string root, DirectorySnapshot snapshot, string globPattern, string excludedFile) {
        var prefix = EntryPrefixFor(root);
        var matcher = new Matcher(PathComparison).AddInclude(globPattern);

        var items = matcher.Match(root, snapshot.Files)
                           .Files
                           .Select(match => SysPath.GetFullPath(SysPath.Combine(root, match.Path)))
                           .Where(file => IsInside(root, file) && !IsSamePath(file, excludedFile))
                           .Distinct(StringComparer.FromComparison(PathComparison))
                           .Select(file => ArchiveItem.ForFile($"{prefix}{ToEntryPath(root, file)}", file))
                           .OrderBy(item => item.EntryName, StringComparer.Ordinal)
                           .ToArray();

        Log.GlobMatched(_logger, globPattern, items.Length, root);

        return items;
    }

    /// <summary>
    ///     Rooted patterns (<c>/x</c>, <c>\x</c>, <c>\\server\share</c>,
    ///     <c>C:x</c> on Windows) and <c>..</c> segments are refused: they
    ///     could only aim outside the directory, so they are caller mistakes,
    ///     not empty matches.
    /// </summary>
    private static void ValidateGlobPattern(string globPattern) {
        Throws.When.NullOrWhiteSpace(globPattern);

        var rooted = globPattern[0] is Separators.ForwardSlash or
                                       Separators.BackwardSlash ||
                     SysPath.IsPathRooted(globPattern);

        if (rooted || globPattern.Split(PathHelper.PathSeparators).Contains(PARENT_SEGMENT)) {
            throw new ArgumentException(
                message: $"The glob pattern '{globPattern}' must stay inside the directory: rooted patterns and '..' segments are not allowed.",
                paramName: nameof(globPattern)
            );
        }
    }

    private static string EntryPrefixFor(string directory) {
        return SysPath.GetFileName(directory) is { Length: > 0 } name
            ? $"{name}{Separators.ForwardSlash}"
            : string.Empty;
    }

    private static string ToEntryPath(string root, string fullPath) {
        return SysPath.GetRelativePath(root, fullPath)
                      .Replace(SysPath.DirectorySeparatorChar, Separators.ForwardSlash);
    }

    private static bool IsInside(string root, string fullPath) {
        var relative = SysPath.GetRelativePath(root, fullPath);

        return !SysPath.IsPathRooted(relative)
            && relative != ".."
            && !relative.StartsWith(".." + SysPath.DirectorySeparatorChar, StringComparison.Ordinal);
    }

    private static bool IsSamePath(string left, string right) {
        return string.Equals(left, right, PathComparison);
    }

    private static void TryDelete(string path) {
        try { SysFile.Delete(path); }
        catch (IOException) { /* best effort */ }
        catch (UnauthorizedAccessException) { /* best effort */ }
    }

    /// <param name="EntryName">
    ///     Name inside the archive; ends with <c>/</c> for directories.
    /// </param>
    /// <param name="SourcePath">
    ///     File to read, or <see langword="null"/> for a directory entry.
    /// </param>
    private sealed record ArchiveItem(string EntryName, string? SourcePath) {
        public static ArchiveItem ForFile(string entryName, string sourcePath) {
            return new ArchiveItem(entryName, sourcePath);
        }

        public static ArchiveItem ForDirectory(string entryName) {
            return new ArchiveItem(entryName, null);
        }
    }
}
