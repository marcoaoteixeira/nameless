using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Nameless.Compression.Internals;

namespace Nameless.Backup;

/// <summary>
///     Validated, ordered set of backup routines.
/// </summary>
internal sealed partial class BackupRoutineCatalog {
    private readonly Dictionary<string, IBackupRoutine> _routinesByKey;

    /// <summary>
    ///     Gets the routines ordered by <see cref="IBackupRoutine.Order"/>,
    ///     then <see cref="IBackupRoutine.Key"/> (ordinal).
    /// </summary>
    public IReadOnlyList<IBackupRoutine> Routines { get; }

    /// <exception cref="InvalidOperationException">
    ///     if a routine has an invalid key, or two routines share a key.
    /// </exception>
    public BackupRoutineCatalog(IEnumerable<IBackupRoutine> routines) {
        Throws.When.Null(routines);

        _routinesByKey = new Dictionary<string, IBackupRoutine>(StringComparer.Ordinal);

        foreach (var routine in routines) {
            if (!IsValidKey(routine.Key)) {
                throw new InvalidOperationException(
                    $"Backup routine '{routine.GetType().FullName}' has the invalid key '{routine.Key}'. Keys use lowercase letters, digits, '.', '_' and '-', start and end with a letter or digit, have at most 64 characters and are not Windows reserved device names."
                );
            }

            if (!_routinesByKey.TryAdd(routine.Key, routine)) {
                throw new InvalidOperationException(
                    $"Backup routines '{_routinesByKey[routine.Key].GetType().FullName}' and '{routine.GetType().FullName}' share the key '{routine.Key}'. Keys must be unique."
                );
            }
        }

        Routines = [
            .. _routinesByKey.Values
                             .OrderBy(routine => routine.Order)
                             .ThenBy(routine => routine.Key, StringComparer.Ordinal)
        ];
    }

    public bool TryGet(string key, [NotNullWhen(returnValue: true)] out IBackupRoutine? routine) {
        return _routinesByKey.TryGetValue(key, out routine);
    }

    public static bool IsValidKey([NotNullWhen(returnValue: true)] string? key) {
        return key is not null
               && KeyRegex().IsMatch(key)
               && !WindowsReservedNames.IsReserved(key);
    }

    [GeneratedRegex(pattern: "^[a-z0-9](?:[a-z0-9._-]{0,62}[a-z0-9])?$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyRegex();
}
