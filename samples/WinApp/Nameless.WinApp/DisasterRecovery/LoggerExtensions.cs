using Microsoft.Extensions.Logging;
using Nameless.WinApp.Internals;

namespace Nameless.WinApp.DisasterRecovery;

internal static class LoggerExtensions {
    private static readonly Action<ILogger, Exception> BackupFailureDelegate
        = LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: default,
            formatString: "An error occurred while executing SQLite disaster recovery routine."
        );

    extension(ILogger<SqliteBackupRecoveryRoutine> self) {
        internal void Failure(string actionName, Exception exception) {
            Log.Failure(
                logger: self,
                tag: "SQLITE_DISASTER_RECOVERY",
                actionName,
                exception
            );
        }

        internal void MissingSourceFile(string actionName, string sourceFilePath) {
            Log.Warning(
                logger: self,
                tag: "SQLITE_DISASTER_RECOVERY",
                actionName,
                reason: $"Missing source file. ({sourceFilePath})"
            );
        }

        internal void BackupFailure(Exception exception) {
            BackupFailureDelegate(self, exception);
        }
    }
}
