using Nameless.ObjectModel;

namespace Nameless.Windows.BackupRecovery;

public class RestoreOutput : Result<bool> {
    public static RestoreOutput Ack => new(value: true, errors: []);

    private RestoreOutput(bool? value, Error[] errors)
        : base(value, errors) { }

    public static implicit operator RestoreOutput(bool value) {
        return new RestoreOutput(value, errors: []);
    }
    
    public static implicit operator RestoreOutput(Error error) {
        return new RestoreOutput(value: null, errors: [error]);
    }

    public static implicit operator RestoreOutput(Error[] errors) {
        return new RestoreOutput(value: null, errors);
    }
}