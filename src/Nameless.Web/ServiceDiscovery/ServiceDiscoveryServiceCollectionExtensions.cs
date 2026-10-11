using Microsoft.Extensions.DependencyInjection;
using Nameless.Helpers;

namespace Nameless.Web.ServiceDiscovery;

/// <summary>
///     Host application builder extension methods.
/// </summary>
public static class ServiceDiscoveryServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers discovery services in the application builder.
        /// </summary>
        /// <param name="configure">
        ///     The registration settings delegate.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> instance so other
        ///     actions can be chained.
        /// </returns>
        public IServiceCollection RegisterServiceDiscovery(Action<ServiceDiscoveryRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self
                .AddServiceDiscovery(opts => {
                    if (!registration.OverrideServiceDiscoveryConfiguration) {
                        /* default service discovery configuration */
                    }

                    registration.ConfigureServiceDiscovery?.Invoke(opts);
                })
                .ConfigureHttpClientDefaults(builder => {
                    // Turn on service discovery by default, no opt-out.
                    builder.AddServiceDiscovery();

                    // Turn on resilience by default
                    _ = registration.OverrideHttpStandardResilience
                        ? builder.AddStandardResilienceHandler(
                            resilience => registration.ConfigureHttpStandardResilience?.Invoke(resilience)
                        )
                        : builder.AddStandardResilienceHandler();
                });

            return self;
        }
    }
}