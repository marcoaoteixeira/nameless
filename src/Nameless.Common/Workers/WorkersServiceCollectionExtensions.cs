using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Nameless.Helpers;

namespace Nameless.Workers;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods for background worker services.
/// </summary>
public static class WorkersServiceCollectionExtensions {
    /// <param name="self">The current <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers all configured <see cref="PeriodicWorker"/> types as <see cref="IHostedService"/> singletons.
        /// </summary>
        /// <param name="configure">Optional delegate to configure worker types and assembly scanning.</param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterPeriodicWorkers(Action<PeriodicWorkersRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.TryAddEnumerable(registration.Workers.Select(
                implementation => ServiceDescriptor.Singleton(
                    typeof(IHostedService),
                    implementation
                )
            ));

            return self;
        }
    }
}