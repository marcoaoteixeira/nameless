using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nameless.IO;
using Nameless.Logging;

namespace Nameless.EntityFrameworkCore;

/// <summary>
///     Resource-based Database Seeder
/// </summary>
public abstract class ResourceDatabaseSeeder : IDatabaseSeeder {
    private readonly IFileProvider _fileProvider;

    /// <summary>
    ///     Gets the logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    ///     Gets or sets the resource path.
    /// </summary>
    public abstract string ResourceRelativePath { get; }

    /// <summary>
    ///     Whether it should throw <see cref="MissingDatabaseSeederResourceException"/>
    ///     when resource is missing.
    /// </summary>
    public virtual bool ThrowOnMissing => false;

    /// <inheritdoc/>
    public abstract int Order { get; }

    /// <summary>
    ///     Base class constructor.
    /// </summary>
    /// <param name="fileProvider">
    ///     The file provider.
    /// </param>
    /// <param name="logger">
    ///     The logger.
    /// </param>
    protected ResourceDatabaseSeeder(IFileProvider fileProvider, ILogger logger) {
        _fileProvider = fileProvider;

        Logger = logger;
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(DbContext dbContext, bool storeManagementOperation, CancellationToken cancellationToken) {
        using var logger = Logger.StartStopwatchLogger();

        try {
            var seeds = GetResourceFileContent();

            await ExecuteAsyncCore(dbContext, seeds, storeManagementOperation, cancellationToken).SkipContextSync();
        }
        catch (Exception ex) {
            CommonLog.Error(Logger, ex.Message, ex, GetType().Tag);

            throw;
        }
    }

    /// <inheritdoc/>
    public void Execute(DbContext dbContext, bool storeManagementOperation) {
        using var logger = Logger.StartStopwatchLogger();

        try {
            var resourceFileContent = GetResourceFileContent();

            ExecuteCore(dbContext, resourceFileContent, storeManagementOperation);
        }
        catch (Exception ex) {
            CommonLog.Error(Logger, ex.Message, ex, GetType().Tag);

            throw;
        }
    }

    /// <summary>
    ///     Executes the database seeder async.
    /// </summary>
    /// <param name="dbContext">
    ///     The database context.
    /// </param>
    /// <param name="resourceFileContent">
    ///     The resource file content.
    /// </param>
    /// <param name="storeManagementOperation">
    ///     Whether it should store management operations.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token.
    /// </param>
    /// <returns>
    ///     A <see cref="Task"/> representing the asynchronous method execution.
    /// </returns>
    protected abstract Task ExecuteAsyncCore(DbContext dbContext, string resourceFileContent, bool storeManagementOperation, CancellationToken cancellationToken);

    /// <summary>
    ///     Executes the database seeder.
    /// </summary>
    /// <param name="dbContext">
    ///     The database context.
    /// </param>
    /// <param name="resourceFileContent">
    ///     The resource file content.
    /// </param>
    /// <param name="storeManagementOperation">
    ///     Whether it should store management operations.
    /// </param>
    protected abstract void ExecuteCore(DbContext dbContext, string resourceFileContent, bool storeManagementOperation);

    private string GetResourceFileContent() {
        var file = _fileProvider.GetFile(ResourceRelativePath);

        if (!file.Exists) {
            CommonLog.Debug(
                Logger,
                message: $"Resource file '{ResourceRelativePath}' is missing.",
                tag: GetType().Tag
            );

            return ThrowOnMissing
                ? throw new MissingDatabaseSeederResourceException(ResourceRelativePath)
                : string.Empty;
        }

        using var stream = file.Open(FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var content = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(content)) {
            CommonLog.Debug(
                Logger,
                message: $"Resource file '{ResourceRelativePath}' is empty.",
                tag: GetType().Tag
            );
        }

        return content;
    }
}