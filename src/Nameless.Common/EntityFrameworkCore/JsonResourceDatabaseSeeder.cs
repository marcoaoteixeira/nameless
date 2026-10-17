using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nameless.EntityFrameworkCore.Entities;
using Nameless.IO;

namespace Nameless.EntityFrameworkCore;

/// <summary>
///     JSON resource-based Database Seeder
/// </summary>
public abstract class JsonResourceDatabaseSeeder<TEntity> : ResourceDatabaseSeeder where TEntity : EntityBase {
    /// <summary>
    ///     Gets the JSON serializer options.
    /// </summary>
    public virtual JsonSerializerOptions JsonOptions { get; } = new();

    /// <summary>
    ///     Base class constructor.
    /// </summary>
    /// <param name="fileProvider">
    ///     The file provider.
    /// </param>
    /// <param name="logger">
    ///     The logger.
    /// </param>
    protected JsonResourceDatabaseSeeder(IFileProvider fileProvider, ILogger logger)
        : base(fileProvider, logger) { }

    /// <inheritdoc />
    protected sealed override void ExecuteCore(DbContext dbContext, string resourceFileContent, bool storeManagementOperation) {
        var seeds = GetSeedsFromResourceFileContent(resourceFileContent);

        ExecuteCore(dbContext, seeds, storeManagementOperation);
    }

    /// <summary>
    ///     Executes the database seeder.
    /// </summary>
    /// <param name="dbContext">
    ///     The database context.
    /// </param>
    /// <param name="seeds">
    ///     The seeds.
    /// </param>
    /// <param name="storeManagementOperation">
    ///     Whether it should store management operations.
    /// </param>
    protected abstract void ExecuteCore(DbContext dbContext, TEntity[] seeds, bool storeManagementOperation);

    /// <inheritdoc />
    protected sealed override Task ExecuteAsyncCore(DbContext dbContext, string resourceFileContent, bool storeManagementOperation, CancellationToken cancellationToken) {
        var seeds = GetSeedsFromResourceFileContent(resourceFileContent);

        return ExecuteAsyncCore(dbContext, seeds, storeManagementOperation, cancellationToken);
    }

    /// <summary>
    ///     Executes the database seeder async.
    /// </summary>
    /// <param name="dbContext">
    ///     The database context.
    /// </param>
    /// <param name="seeds">
    ///     The seeds.
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
    protected abstract Task ExecuteAsyncCore(DbContext dbContext, TEntity[] seeds, bool storeManagementOperation, CancellationToken cancellationToken);

    private TEntity[] GetSeedsFromResourceFileContent(string resourceFileContent) {
        if (string.IsNullOrWhiteSpace(resourceFileContent)) { return []; }

        return JsonSerializer.Deserialize<TEntity[]>(resourceFileContent, JsonOptions) ??
               throw new InvalidOperationException(
                   $"Unable to deserialize seeds to type '{typeof(TEntity)}'."
               );
    }
}