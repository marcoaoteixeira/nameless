using Microsoft.AspNetCore.Builder;
using Nameless.Configuration;
using Nameless.Web.HealthCheck.Reporting;
using MS_HealthCheckOptions = Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions;

namespace Nameless.Web.HealthCheck;

/// <summary>
///     <see cref="WebApplication"/> extension methods for configure
///     health checks endpoints.
/// </summary>
public static class HealthCheckWebApplicationExtensions {
    /// <param name="self">
    ///     The current <see cref="WebApplication"/>.
    /// </param>
    extension(WebApplication self) {
        /// <summary>
        ///     Uses health checks endpoints in the application.
        /// </summary>
        /// <returns>
        ///     The current <see cref="WebApplication"/> instance so other
        ///     actions can be chained.
        /// </returns>
        /// <remarks>
        ///     Call this method after <c>UseRouting</c>.
        /// </remarks>
        public WebApplication UseHealthCheck() {
            var healthChecks = self.MapGroup(string.Empty);

            healthChecks
                .DisableRequestTimeout()
                .DisableRateLimiting()
                .AllowAnonymous();

            var config = self.Configuration
                             .GetSection<HealthCheckOptions>()
                             .GetOrThrow<HealthCheckOptions>();
            
            // Liveness: no checks at all, just "process is alive" (cheap, safe)
            healthChecks.MapHealthChecks(
                pattern: config.LivenessPath,
                options: new MS_HealthCheckOptions {
                    Predicate = registration => registration.Tags.Contains(HealthCheckConstants.Tags.Alive),
                    ResponseWriter = JsonReportWriter.WriteAsync
                }
            );

            // Readiness: real dependency checks, tagged
            healthChecks.MapHealthChecks(
                pattern: config.ReadinessPath,
                options: new MS_HealthCheckOptions {
                    Predicate = registration => registration.Tags.Overlaps(config.ReadinessTags),
                    ResponseWriter = JsonReportWriter.WriteAsync
                }
            );

            return self;
        }
    }
}