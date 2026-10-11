using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Helpers;
using Scalar.AspNetCore;

namespace Nameless.Web.Scalar;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class ScalarWebApplicationExtensions {
    /// <param name="self">
    ///     The current instance of <see cref="WebApplication"/>.
    /// </param>
    extension(WebApplication self) {
        /// <summary>
        ///     Configures the Scalar API for the current web application
        ///     using the specified registration settings.
        /// </summary>
        /// <param name="configure">
        ///     An optional action that configures the Scalar registration
        ///     settings. If not specified, default settings are applied.
        /// </param>
        /// <returns>
        ///     The current instance of the <see cref="WebApplication"/> to
        ///     enable method chaining.
        /// </returns>
        public WebApplication UseScalar(Action<ScalarRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);
            var combination = Delegate.Combine(DefaultConfig, registration.ConfigureScalarOptions);

            self.MapScalarApiReference((Action<ScalarOptions, HttpContext>)combination);

            return self;

            void DefaultConfig(ScalarOptions options, HttpContext httpContext) {
                options
                    .WithTitle(self.Environment.ApplicationName)
                    .WithTheme(ScalarTheme.BluePlanet)
                    .WithDefaultHttpClient(ScalarTarget.Shell, ScalarClient.Curl)
                    .SortOperationsByMethod();
            }
        }
    }
}
