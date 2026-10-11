using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.Application;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods
/// </summary>
public static class ApplicationServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection" />.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers <see cref="IApplicationContext" /> implementation in the
        ///     service collection.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection" />, so other actions can
        ///     be chained.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     if
        ///         <paramref name="self"/> or
        ///         <paramref name="configuration"/>
        ///     is <see langword="null"/>.
        /// </exception>
        public IServiceCollection RegisterApplicationContext(IConfiguration? configuration = null) {
            self.ConfigureOptions<ApplicationContextOptions>(configuration)
                .TryAddSingleton<IApplicationContext, ApplicationContext>();

            return self;
        }
    }
}