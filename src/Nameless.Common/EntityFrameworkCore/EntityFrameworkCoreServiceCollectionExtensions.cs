using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.EntityFrameworkCore.Config;
using Nameless.Helpers;

namespace Nameless.EntityFrameworkCore;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods for Entity Framework Core.
/// </summary>
public static class EntityFrameworkCoreServiceCollectionExtensions {
    /// <param name="self">The current <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers Entity Framework Core services, interceptors, and the
        ///     specified <typeparamref name="TDbContext"/>.
        /// </summary>
        /// <typeparam name="TDbContext">The <see cref="DbContext"/> type to register.</typeparam>
        /// <param name="configure">Optional delegate to configure interceptors and seeder.</param>
        /// <param name="configuration">Optional configuration for <see cref="EntityFrameworkCoreOptions"/>.</param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterEntityFrameworkCore<TDbContext>(Action<EntityFrameworkCoreRegistration>? configure = null, IConfiguration? configuration = null)
            where TDbContext : DbContext {
            var registration = ActionHelper.FromDelegate(configure);

            self.ConfigureOptions<EntityFrameworkCoreOptions>(configuration);

            self.RegisterInterceptors(registration);
            self.RegisterDataSeeders(registration);

            var configureDbContext = (Action<IServiceProvider, DbContextOptionsBuilder>)Delegate.Combine(
                DefaultConfiguration,
                registration.DbContextConfiguration ?? SqliteDbContextConfiguration
            );

            return registration.UseDbContextFactory
                ? self.AddDbContextFactory<TDbContext>(configureDbContext)
                : self.AddDbContext<TDbContext>(configureDbContext);
        }

        /// <summary>
        ///     Registers the entity type configuration for a given entity.
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Type of the entity.
        /// </typeparam>
        /// <typeparam name="TEntityTypeConfiguration">
        ///     Type of the entity type configuration, related to the entity.
        /// </typeparam>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterEntityTypeConfiguration<TEntity, TEntityTypeConfiguration>()
            where TEntity : class
            where TEntityTypeConfiguration : class, IEntityTypeConfiguration<TEntity> {
            self.AddSingleton<TEntityTypeConfiguration>();
            self.AddSingleton<IEntityTypeConfigurator>(provider => new EntityTypeConfigurator<TEntity>(
                provider.GetRequiredService<TEntityTypeConfiguration>()
            ));

            return self;
        }

        private void RegisterInterceptors(EntityFrameworkCoreRegistration registration) {
            self.TryAddEnumerable(registration.Interceptors.Select(
                implementation => ServiceDescriptor.Transient(typeof(IInterceptor), implementation)
            ));
        }

        private void RegisterDataSeeders(EntityFrameworkCoreRegistration registration) {
            const string Key = "::database:seeder::dd499dc3-f896-4030-ba59-00f8bb2647e9";

            self.TryAddTransient<DatabaseSeederAggregator>(provider => new DatabaseSeederAggregator(
                seeders: provider.GetKeyedServices<IDatabaseSeeder>(Key),
                logger: provider.GetLogger<DatabaseSeederAggregator>()
            ));

            self.TryAddEnumerable(
                registration.DatabaseSeeders.Select(
                    implementation => ServiceDescriptor.KeyedTransient(
                        typeof(IDatabaseSeeder), Key, implementation
                    )
                )
            );
        }
    }
    
    private static void SqliteDbContextConfiguration(IServiceProvider provider, DbContextOptionsBuilder builder) {
        // NOTE: If using Aspire, it's possible to retrieve the
        // connection string being used by the database resource in
        // Aspire. Aspire will automatically inject the connection
        // strings in the "ConnectionStrings" configuration section
        // for the given resource name.
        var options = provider.GetOptions<EntityFrameworkCoreOptions>().Value;
        var connStr = provider.GetRequiredService<IConfiguration>()
                              .GetConnectionString(options.ConnectionStringName);

        builder.UseSqlite(connStr);
    }

    private static void DefaultConfiguration(IServiceProvider provider, DbContextOptionsBuilder builder) {
        builder.AddInterceptors(
            provider.GetServices<IInterceptor>()
        );

        var aggregator = provider.GetRequiredService<DatabaseSeederAggregator>();
        builder.UseAsyncSeeding(aggregator.ExecuteAsync)
               .UseSeeding(aggregator.Execute);
    }
}
