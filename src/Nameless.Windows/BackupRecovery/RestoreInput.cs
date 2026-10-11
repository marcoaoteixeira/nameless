namespace Nameless.Windows.BackupRecovery;

public record RestoreInput {
    public required string SourceDirectoryPath { get; init; }
    public required string DestinationDirectoryPath { get; init; }
}