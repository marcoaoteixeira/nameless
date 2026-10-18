using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Abstractions.Controls;

namespace Nameless.Windows.Navigation;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class ServiceCollectionExtensions {
    extension(IServiceCollection self) {
        public IServiceCollection RegisterNavigation(Action<NavigationRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.RegisterNavigationService();
            self.RegisterNavigationWindow(registration);
            self.RegisterNavigationViewItemProvider(registration);
            self.RegisterNavigableViews(registration);

            return self;
        }

        private void RegisterNavigationService() {
            // Navigation services for WPF-UI
            self.TryAddSingleton<INavigationViewPageProvider, NavigationViewPageProvider>();
            self.TryAddSingleton<INavigationService, NavigationService>();
        }

        private void RegisterNavigationWindow(NavigationRegistration registration) {
            if (registration.NavigationWindow is null) {
                throw new InvalidOperationException(
                    $"Unable to locate implementation for type '{nameof(INavigationWindow)}'."
                );
            }

            self.TryAdd(
                ServiceDescriptor.Transient(
                    typeof(INavigationWindow),
                    registration.NavigationWindow
                )
            );
        }

        private void RegisterNavigationViewItemProvider(NavigationRegistration registration) {
            self.TryAddSingleton<INavigationViewItemProvider>(
                new AutoDiscoverableNavigationViewItemProvider(registration.Assemblies)
            );
        }

        private void RegisterNavigableViews(NavigationRegistration registration) {
            var service = typeof(INavigableView<>);

            foreach (var implementation in registration.NavigationViews) {
                var interfaces = implementation.GetInterfaces()
                                               .Where(@interface => @interface.GenericTypeArguments.Length > 0 &&
                                                                    service.IsAssignableFromGeneric(@interface));
                foreach (var @interface in interfaces) {
                    // Register the navigable view's interface.
                    self.TryAdd(ServiceDescriptor.Transient(@interface, implementation));

                    // Register the navigable view viewmodel.
                    var viewModelType = @interface.GetGenericArguments().First();
                    self.TryAdd(ServiceDescriptor.Transient(viewModelType, viewModelType));
                }

                // Register the navigable view concrete type.
                self.TryAdd(ServiceDescriptor.Transient(implementation, implementation));
            }
        }
    }
}