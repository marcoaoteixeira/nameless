using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.Data.SqlServer;

/// <summary>
///     Extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class SqlServerServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the data services.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterSqlServerDatabase(IConfiguration? configuration = null) {
            self.ConfigureOptions<SqlServerOptions>(configuration);

            self.TryAddSingleton<IDbConnectionFactory, DbConnectionFactory>();
            self.TryAddTransient<IDatabase, Database>();

            return self;
        }
    }
}