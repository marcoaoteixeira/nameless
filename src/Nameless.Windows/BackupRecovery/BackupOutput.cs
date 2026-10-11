using Nameless.ObjectModel;

namespace Nameless.Windows.BackupRecovery;

public class BackupOutput : Result<string[]> {
    public static BackupOutput Empty => new(files: [], errors: []);

    private BackupOutput(string[] files, Error[] errors)
        : base (value: files, errors) { }

    public static implicit operator BackupOutput(string file) {
        return new BackupOutput(files: [file], errors: []);
    }

    public static implicit operator BackupOutput(string[] files) {
        return new BackupOutput(files, errors: []);
    }

    public static implicit operator BackupOutput(Error error) {
        return new BackupOutput(files: [], errors: [error]);
    }

    public static implicit operator BackupOutput(Error[] errors) {
        return new BackupOutput(files: [], errors);
    }
}