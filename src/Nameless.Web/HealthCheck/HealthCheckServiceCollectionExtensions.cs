using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nameless.Helpers;
using MS_HealthCheckRegistration = Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration;

namespace Nameless.Web.HealthCheck;

/// <summary>
///     Host application builder extension methods.
/// </summary>
public static class HealthCheckServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers health checks for the application.
        /// </summary>
        /// <param name="configure">
        ///     The registration settings delegate.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> instance so other
        ///     actions can be chained.
        /// </returns>
        public IServiceCollection RegisterHealthCheck(Action<HealthCheckRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            var builder = self.AddHealthChecks();

            // Add other health checks.
            foreach (var healthCheck in registration.HealthChecks) {
                self.TryAddScoped(healthCheck.Type);

                builder.Add(new MS_HealthCheckRegistration(
                    healthCheck.Name,
                    provider => (IHealthCheck)provider.GetRequiredService(healthCheck.Type),
                    healthCheck.FailureStatus,
                    healthCheck.Tags,
                    healthCheck.Timeout
                ));
            }

            // Add a default liveness check to ensure app is responsive
            builder.AddCheck(
                name: "self",
                check: () => HealthCheckResult.Healthy(),
                tags: [HealthCheckConstants.Tags.Alive] // use tag to avoid call other configured health checks.
            );

            return self;
        }
    }
}