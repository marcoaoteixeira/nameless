using System.Text.Json;
using System.Text.RegularExpressions;
using Nameless.Backup.Internals;
using Nameless.ObjectModel;

namespace Nameless.Backup;

/// <summary>
///     Reads and writes <see cref="BackupManifest"/> as JSON. Reading
///     validates the manifest, since it comes from an archive that may be
///     corrupt or crafted.
/// </summary>
internal static partial class BackupManifestSerializer {
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web) {
        WriteIndented = true
    };

    public static async Task WriteAsync(BackupManifest manifest, string filePath, CancellationToken cancellationToken) {
        var stream = SysFile.Create(filePath);

        await using (stream.ConfigureAwait(false)) {
            await JsonSerializer.SerializeAsync(
                stream,
                manifest,
                Options,
                cancellationToken
            ).SkipContextSync();
        }
    }

    /// <returns>
    ///     The manifest, or an <see cref="ErrorType.Validation"/> error if
    ///     it is missing, malformed, from an unsupported format version or
    ///     describes unsafe paths.
    /// </returns>
    public static async Task<BackupManifestResult> ReadAsync(string filePath, CancellationToken cancellationToken) {
        if (!SysFile.Exists(filePath)) {
            return Error.Validation("The archive has no manifest.");
        }

        BackupManifest? manifest;

        try {
            var stream = SysFile.OpenRead(filePath);

            await using (stream.ConfigureAwait(false)) {
                manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(
                    stream,
                    Options,
                    cancellationToken
                ).ConfigureAwait(false);
            }
        }
        catch (JsonException ex) {
            return Error.Validation($"The archive manifest is not valid JSON: {ex.Message}", ex: ex);
        }

        if (manifest is null) {
            return Error.Validation("The archive manifest is empty.");
        }

        var problem = FindProblem(manifest);

        return problem is null
            ? manifest
            : Error.Validation(problem);
    }

    private static string? FindProblem(BackupManifest manifest) {
        if (manifest.FormatVersion != BackupManifest.CurrentFormatVersion) {
            return $"The archive format version {manifest.FormatVersion} is not supported (supported: {BackupManifest.CurrentFormatVersion}).";
        }

        // JSON may leave non-nullable members null
        if (manifest.Routines is null || manifest.Routines.Count == 0) {
            return "The archive manifest lists no routines.";
        }

        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var routine in manifest.Routines) {
            if (!BackupRoutineCatalog.IsValidKey(routine.Key)) {
                return $"The archive manifest has an invalid routine key '{routine.Key}'.";
            }

            if (!keys.Add(routine.Key)) {
                return $"The archive manifest lists the routine '{routine.Key}' more than once.";
            }

            if (FindProblem(routine) is { } problem) {
                return problem;
            }
        }

        return null;
    }

    private static string? FindProblem(BackupManifestRoutine routine) {
        if (routine.Files is null || routine.Files.Count == 0) {
            return $"The archive manifest has no file list for the routine '{routine.Key}'.";
        }

        // case-insensitive: two names differing by case collide on Windows
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in routine.Files) {
            if (!IsSafeRelativePath(file.Path)) {
                return $"The archive manifest has an unsafe path '{file.Path}' for the routine '{routine.Key}'.";
            }

            if (!paths.Add(file.Path)) {
                return $"The archive manifest lists '{routine.Key}/{file.Path}' more than once.";
            }

            if (file.Length < 0 || !Sha256Regex().IsMatch(file.Sha256)) {
                return $"The archive manifest has an invalid length or hash for '{routine.Key}/{file.Path}'.";
            }
        }

        return null;
    }

    private static bool IsSafeRelativePath(string? path) {
        // forward slashes only; no root, drive, stream (':') or backslash;
        // no empty, '.' or '..' segment
        return !string.IsNullOrEmpty(path)
               && !path.Contains('\\')
               && !path.Contains(':')
               && path.Split('/').All(segment => segment.Length > 0 && segment is not "." and not "..");
    }

    [GeneratedRegex(pattern: "^[0-9a-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Regex();
}
