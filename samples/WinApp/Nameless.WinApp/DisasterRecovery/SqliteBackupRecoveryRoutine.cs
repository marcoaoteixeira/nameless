using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nameless.Application;
using Nameless.ObjectModel;
using Nameless.WinApp.Data;
using Nameless.Windows.BackupRecovery;
using Nameless.Windows.Localization;
using Nameless.Windows.Messaging;

namespace Nameless.WinApp.DisasterRecovery;

public class SqliteBackupRecoveryRoutine : BackupRecoveryRoutineBase<SqliteBackupRecoveryRoutine> {
    private const string CLASS = nameof(SqliteBackupRecoveryRoutine);

    private readonly IApplicationContext _applicationContext;
    private readonly AppDbContext _dbContext;

    private ILocalizer T { get; }

    public override string Name => T["SqliteDisasterRecoveryRoutine_Name"];

    public SqliteBackupRecoveryRoutine(
        IApplicationContext applicationContext,
        AppDbContext dbContext,
        ILocalizer localizer,
        IMessenger messenger,
        ILogger<SqliteBackupRecoveryRoutine> logger
        ) : base(messenger, logger) {
        _applicationContext = applicationContext;
        _dbContext = dbContext;

        T = localizer;
    }

    public override async Task<BackupOutput> BackupAsync(BackupInput input, CancellationToken cancellationToken) {
        const string ActionName = nameof(BackupAsync);

        var result = await ExecuteBackupOrRestoreActionAsync(
            ActionName,
            ActionType.Backup,
            input.SourceDirectoryPath,
            input.DestinationDirectoryPath,
            cancellationToken
        ).SkipContextSync();

        return result.Match<BackupOutput>(
            onSuccess: value => value,
            onFailure: failure => failure
        );
    }

    public override async Task<RestoreOutput> RestoreAsync(RestoreInput input, CancellationToken cancellationToken) {
        const string ActionName = nameof(RestoreAsync);

        var result = await ExecuteBackupOrRestoreActionAsync(
            ActionName,
            ActionType.Restore,
            input.SourceDirectoryPath,
            input.DestinationDirectoryPath,
            cancellationToken
        ).SkipContextSync();

        return result.Match<RestoreOutput>(
            onSuccess: _ => RestoreOutput.Ack,
            onFailure: failure => failure
        );
    }

    private async Task<Result<string>> ExecuteBackupOrRestoreActionAsync(
        string actionName,
        ActionType actionType,
        string sourceDirectoryPath,
        string destinationDirectoryPath,
        CancellationToken cancellationToken
    ) {
        await NotifyInformationAsync(T[$"{CLASS}_{actionName}_Starting"]).SkipContextSync();

        var closeAppDbContextConnectionResult = await CloseAppDbContextConnectionAsync().SkipContextSync();
        if (closeAppDbContextConnectionResult.Failure) { return closeAppDbContextConnectionResult.Errors; }

        var sourceFilePathResult = await GetSqliteSourceFilePathAsync(sourceDirectoryPath).SkipContextSync();
        if (sourceFilePathResult.Failure) { return sourceFilePathResult.Errors; }
        
        var destinationFilePathResult = await GetSqliteDestinationFilePathAsync(destinationDirectoryPath, actionType).SkipContextSync();
        if (destinationFilePathResult.Failure) { return destinationFilePathResult.Errors; }

        var backupResult = await ExecuteDatabaseBackupAsync(sourceFilePathResult.Value, destinationFilePathResult.Value, cancellationToken).SkipContextSync();
        if (backupResult.Failure) { return backupResult.Errors; }

        await NotifySuccessAsync(
            string.Format(T[$"{CLASS}_{actionName}_Success"], destinationFilePathResult.Value)
        ).SkipContextSync();

        // ok, everything is fine.
        return destinationFilePathResult.Value;
    }
    
    private async Task<Result<Nothing>> CloseAppDbContextConnectionAsync() {
        const string ActionName = nameof(CloseAppDbContextConnectionAsync);

        await NotifyInformationAsync(
            T[$"{CLASS}_{ActionName}_Starting"]
        ).SkipContextSync();

        try {
            // since we're using EFCore, we first need to close the current
            // DbContext connection
            await _dbContext.Database.CloseConnectionAsync().SkipContextSync();

            return Nothing.Value;
        }
        catch (Exception ex) {
            var message = string.Format(
                T[$"{CLASS}_{ActionName}_Failure"],
                ex.Message
            );

            await NotifyFailureAsync(message).SkipContextSync();

            return Error.Failure(message);
        }
    }

    private async Task<Result<string>> GetSqliteSourceFilePathAsync(string sourceDirectoryPath) {
        const string ActionName = nameof(GetSqliteSourceFilePathAsync);

        await NotifyInformationAsync(
            T[$"{CLASS}_{ActionName}_Starting"]
        ).SkipContextSync();

        var filePath = Path.Combine(sourceDirectoryPath, SqliteConstants.DatabaseFileName);
        var file = _applicationContext.ApplicationDataFileProvider.GetFile(filePath);

        if (file.Exists) { return file.Path; }

        // If the database file does not exist, it might be because
        // the database was not properly initialized.
        Logger.MissingSourceFile(ActionName, file.Path);

        var message = string.Format(
            T[$"{CLASS}_{ActionName}_Missing"],
            file.Path
        );

        await NotifyWarningAsync(message).SkipContextSync();

        return Error.Conflict(message);
    }

    private async Task<Result<string>> GetSqliteDestinationFilePathAsync(string destinationDirectoryPath, ActionType actionType) {
        const string ActionName = nameof(GetSqliteDestinationFilePathAsync);

        await NotifyInformationAsync(
            T[$"{CLASS}_{ActionName}_Starting"]
        ).SkipContextSync();

        var filePath = Path.Combine(destinationDirectoryPath, SqliteConstants.DatabaseFileName);
        var file = _applicationContext.ApplicationDataFileProvider.GetFile(filePath);

        if (file.Exists || actionType == ActionType.Backup) {
            return file.Path;
        }

        // If the database file does not exist, it might be because
        // the database was not properly initialized.
        Logger.MissingSourceFile(ActionName, file.Path);

        var message = string.Format(
            T[$"{CLASS}_{ActionName}_Missing"],
            file.Path
        );

        await NotifyWarningAsync(message).SkipContextSync();

        return Error.Conflict(message);
    }
    
    private async Task<Result<Nothing>> ExecuteDatabaseBackupAsync(string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken) {
        // creates the backup, copying the source file to the destination file.
        const string ActionName = nameof(ExecuteDatabaseBackupAsync);

        SqliteConnection? sourceDbConnection = null;
        SqliteConnection? destinationDbConnection = null;

        await NotifyInformationAsync(
            T[$"{CLASS}_{ActionName}_Starting"]
        ).SkipContextSync();

        try {
            sourceDbConnection = await CreateConnectionAsync(
                sourceFilePath,
                cancellationToken
            ).SkipContextSync();

            destinationDbConnection = await CreateConnectionAsync(
                destinationFilePath,
                cancellationToken
            ).SkipContextSync();

            sourceDbConnection.BackupDatabase(destinationDbConnection);

            return Nothing.Value;
        }
        catch (Exception ex) {
            Logger.Failure(ActionName, ex);

            var message = string.Format(
                T[$"{CLASS}_{ActionName}_Failure"],
                ex.Message
            );

            await NotifyFailureAsync(message).SkipContextSync();

            return Error.Failure(message);
        }
        finally {
            if (destinationDbConnection is not null) {
                await destinationDbConnection.CloseAsync().SkipContextSync();
                await destinationDbConnection.DisposeAsync();
            }

            if (sourceDbConnection is not null) {
                await sourceDbConnection.CloseAsync().SkipContextSync();
                await sourceDbConnection.DisposeAsync();
            }
        }
    }

    private static async Task<SqliteConnection> CreateConnectionAsync(string filePath, CancellationToken cancellationToken) {
        var connStr = string.Format(SqliteConstants.ConnStrPattern, filePath);
        var result = new SqliteConnection(connStr);

        await result.OpenAsync(cancellationToken)
                    .SkipContextSync();

        return result;
    }

    private enum ActionType {
        Backup,

        Restore
    }
}
