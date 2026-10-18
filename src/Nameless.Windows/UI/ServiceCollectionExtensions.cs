using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;
using Nameless.Windows.UI.Impl;

namespace Nameless.Windows.UI;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the window services.
        /// </summary>
        /// <param name="configure">
        ///     Configure action for <see cref="WindowFactoryRegistration"/>.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterWindowFactory(Action<WindowFactoryRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.TryAdd(
                descriptors: CreateWindowServiceDescriptors(registration)
            );

            self.TryAddSingleton<IWindowFactory, WindowFactory>();

            return self;
        }
    }

    private static IEnumerable<ServiceDescriptor> CreateWindowServiceDescriptors(WindowFactoryRegistration settings) {
        var service = typeof(IWindow);
        
        foreach (var implementation in settings.Windows) {
            var interfaces = implementation.GetInterfaces()
                                           .Where(service.IsAssignableFrom);

            foreach (var @interface in interfaces) {
                // Skip the main service interface
                if (@interface == service) { continue; }

                // Register the service with its extended interface
                yield return ServiceDescriptor.Transient(@interface, implementation);
            }

            // Register the service as concrete type
            yield return ServiceDescriptor.Transient(implementation, implementation);
        }
    }
}