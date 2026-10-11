namespace Nameless.Windows.BackupRecovery;

public record BackupInput {
    public required string SourceDirectoryPath { get; set; }
    public required string DestinationDirectoryPath { get; set; }
}