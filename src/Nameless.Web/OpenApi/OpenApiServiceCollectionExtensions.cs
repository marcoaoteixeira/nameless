using Microsoft.Extensions.DependencyInjection;
using Nameless.Helpers;

namespace Nameless.Web.OpenApi;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods
/// </summary>
public static class OpenApiServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/> instance.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers OpenAPI services for the application using the
        ///     specified registration settings.
        /// </summary>
        /// <remarks>
        ///     If no document options are specified in the registration
        ///     settings, the default OpenAPI services are added.
        ///     Otherwise, each provided document option is registered
        ///     individually.
        /// </remarks>
        /// <param name="configure">
        ///     An action that configures the OpenAPI registration settings,
        ///     including document options for the API. Cannot be null.
        /// </param>
        /// <returns>
        ///     The current instance of the <see cref="IServiceCollection"/>
        ///     so other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterOpenApi(Action<OpenApiRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            if (registration.DocumentOptions.Count == 0) {
                self.AddOpenApi();

                return self;
            }

            foreach (var document in registration.DocumentOptions) {
                if (string.IsNullOrWhiteSpace(document.Key)) { self.AddOpenApi(document.Value); }
                else { self.AddOpenApi(document.Key, document.Value); }
            }

            return self;
        }
    }
}