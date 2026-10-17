using Nameless.Compression;
using Nameless.ObjectModel;

namespace Nameless.Backup.Internals;

/// <summary>
///     Extracts a backup archive and reads its verified manifest. Nothing
///     here touches live state.
/// </summary>
internal sealed class BackupArchiveReader {
    private readonly ICompressor _compressor;

    public BackupArchiveReader(ICompressor compressor) {
        _compressor = Throws.When.Null(compressor);
    }

    /// <summary>
    ///     Extracts the archive, verifying its checksum when given.
    /// </summary>
    /// <param name="archiveFilePath">
    ///     The archive.
    /// </param>
    /// <param name="checksum">
    ///     The expected archive checksum, already validated, or
    ///     <see langword="null"/>.
    /// </param>
    /// <param name="destinationDirectoryPath">
    ///     The directory to extract into.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token.
    /// </param>
    /// <returns>
    ///     A validation error for a checksum mismatch or an invalid archive,
    ///     a missing error when the archive is gone, a failure for unsafe
    ///     entries or I/O errors.
    /// </returns>
    public async Task<EmptyResult> ExtractAsync(string archiveFilePath, string? checksum, string destinationDirectoryPath, CancellationToken cancellationToken) {
        try {
            await _compressor.DecompressAsync(
                archiveFilePath, 
                destinationDirectoryPath, 
                checksum, 
                overwrite: false, 
                cancellationToken
            ).SkipContextSync();

            return Nothing.Value;
        }
        catch (ChecksumMismatchException ex) {
            return Error.Validation($"The archive '{archiveFilePath}' does not match the expected checksum. Nothing was restored.", ex: ex);
        }
        catch (InvalidDataException ex) {
            return Error.Validation($"The file '{archiveFilePath}' is not a valid backup archive: {ex.Message}", ex: ex);
        }
        catch (FileNotFoundException ex) {
            // the archive was removed after the caller checked it
            return Error.Missing($"The archive '{archiveFilePath}' was not found.", ex: ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
            // unsafe entries (path traversal, reserved names) surface as IOException
            return Error.Failure($"The archive '{archiveFilePath}' could not be extracted: {ex.Message}", ex: ex);
        }
    }

    /// <summary>
    ///     Reads the manifest of an extracted archive and verifies the
    ///     payload against it.
    /// </summary>
    /// <param name="extractedDirectoryPath">
    ///     The directory the archive was extracted into; it must hold the
    ///     payload directory only.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token.
    /// </param>
    /// <returns>
    ///     The manifest, or one validation error per problem found.
    /// </returns>
    public static async Task<BackupManifestResult> ReadManifestAsync(string extractedDirectoryPath, CancellationToken cancellationToken) {
        var unexpected = SysDirectory.EnumerateFileSystemEntries(extractedDirectoryPath)
                                     .Select(SysPath.GetFileName)
                                     .Where(name => name != BackupManifest.PayloadDirectoryName)
                                     .Order(StringComparer.Ordinal)
                                     .ToArray();

        if (unexpected.Length > 0) {
            return Error.Validation($"The archive holds unexpected top-level entries: {string.Join(", ", unexpected)}.");
        }

        var payload = SysPath.Combine(extractedDirectoryPath, BackupManifest.PayloadDirectoryName);
        var manifest = await BackupManifestSerializer.ReadAsync(
            SysPath.Combine(payload, BackupManifest.FileName),
            cancellationToken
        ).SkipContextSync();

        if (!manifest.Success) { return manifest.Errors; }

        var verification = await BackupManifestVerifier.VerifyAsync(
            manifest.Value,
            payload,
            cancellationToken
        ).SkipContextSync();

        return verification.Success
            ? manifest.Value
            : verification.Errors;
    }
}
