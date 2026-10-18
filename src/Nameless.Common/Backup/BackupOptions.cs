using System.IO.Compression;
using Nameless.Configuration;

namespace Nameless.Backup;

/// <summary>
///     Backup &amp; restore options.
/// </summary>
[ConfigurationSectionName("Backup")]
public sealed class BackupOptions {
    /// <summary>
    ///     Gets or sets the directory where operations stage their files
    ///     (one subdirectory per operation, deleted afterward). It must
    ///     have room for the backed up data. Default
    ///     <c>%TEMP%/Backup</c>.
    /// </summary>
    /// <exception cref="ArgumentException">
    ///     if the value is <see langword="null"/>, empty or white space.
    /// </exception>
    public string WorkingDirectoryPath {
        get;
        set => field = Throws.When.NullOrWhiteSpace(value, paramName: nameof(WorkingDirectoryPath));
    } = SysPath.Combine(SysPath.GetTempPath(), "Backup");

    /// <summary>
    ///     Gets or sets the archive compression level. Default
    ///     <see cref="CompressionLevel.Optimal"/>.
    /// </summary>
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Optimal;
}
