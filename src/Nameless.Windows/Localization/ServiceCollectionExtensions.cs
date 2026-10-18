using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;

namespace Nameless.Windows.Localization;

/// <summary>
///     Extension methods for registering localization services on
///     <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions {
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the <see cref="ILocalizer"/> service.
        ///     The concrete implementation is resolved via
        ///     <paramref name="configure"/> (explicit type or assembly
        ///     scan). Falls back to <see cref="NullLocalizer"/> when no
        ///     implementation is found.
        /// </summary>
        /// <param name="configure">
        ///     Optional delegate to configure
        ///     <see cref="LocalizationRegistration"/>.
        /// </param>
        /// <param name="configuration">
        ///     Optional <see cref="IConfiguration"/> section bound to
        ///     <see cref="ResourceLocalizerOptions"/>.
        /// </param>
        /// <returns>
        ///     The service collection for further chaining.
        /// </returns>
        public IServiceCollection RegisterLocalization(Action<LocalizationRegistration>? configure = null, IConfiguration? configuration = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.ConfigureOptions<ResourceLocalizerOptions>(configuration);

            var descriptor = registration.Localizer is not null
                ? ServiceDescriptor.Singleton(typeof(ILocalizer), registration.Localizer)
                : ServiceDescriptor.Singleton(NullLocalizer.Instance);

            self.TryAddSingleton(descriptor);

            return self;
        }
    }
}