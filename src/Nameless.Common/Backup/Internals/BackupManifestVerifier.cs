using Nameless.ObjectModel;

namespace Nameless.Backup.Internals;

/// <summary>
///     Checks an extracted payload against its manifest.
/// </summary>
internal static class BackupManifestVerifier {
    /// <param name="manifest">
    ///     A manifest already validated by
    ///     <see cref="BackupManifestSerializer.ReadAsync"/>.
    /// </param>
    /// <param name="payloadDirectoryPath">
    ///     The payload directory (holds the manifest and one folder per
    ///     routine).
    /// </param>
    /// <param name="cancellationToken">
    ///     Token to cancel the operation.
    /// </param>
    /// <returns>
    ///     <see cref="Nothing"/> when the payload holds exactly the listed
    ///     files with the listed lengths and hashes; one
    ///     <see cref="ErrorType.Validation"/> error per problem otherwise.
    /// </returns>
    public static async Task<EmptyResult> VerifyAsync(BackupManifest manifest, string payloadDirectoryPath, CancellationToken cancellationToken) {
        var expected = manifest.Routines
                               .SelectMany(routine => routine.Files.Select(file => (Path: $"{routine.Key}/{file.Path}", File: file)))
                               .ToDictionary(item => item.Path, item => item.File, StringComparer.Ordinal);

        var actual = SysDirectory.EnumerateFiles(payloadDirectoryPath, "*", SearchOption.AllDirectories)
                                 .Select(path => BackupManifestBuilder.ToRelativePath(payloadDirectoryPath, path))
                                 .Where(path => path != BackupManifest.FileName)
                                 .ToHashSet(StringComparer.Ordinal);

        var errors = new List<Error>();

        errors.AddRange(
            expected.Keys
                    .Where(path => !actual.Contains(path))
                    .Order(StringComparer.Ordinal)
                    .Select(path => Error.Validation($"The archive is missing '{path}', listed in its manifest."))
        );

        errors.AddRange(
            actual.Where(path => !expected.ContainsKey(path))
                  .Order(StringComparer.Ordinal)
                  .Select(path => Error.Validation($"The archive holds '{path}', which its manifest does not list."))
        );

        foreach (var (path, file) in expected.Where(item => actual.Contains(item.Key))) {
            var fullPath = SysPath.Combine(payloadDirectoryPath, path);
            var (length, sha256) = await BackupManifestBuilder.HashFileAsync(fullPath, cancellationToken).SkipContextSync();

            if (length != file.Length || !string.Equals(sha256, file.Sha256, StringComparison.Ordinal)) {
                errors.Add(Error.Validation($"The content of '{path}' does not match the archive manifest (corrupt or altered)."));
            }
        }

        return errors.Count == 0 ? Nothing.Value : errors.ToArray();
    }
}
