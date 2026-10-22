using Microsoft.AspNetCore.Builder;
using Nameless.Web.HealthCheck;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the health checks and maps the liveness and readiness endpoints.
/// </summary>
public class HealthCheckFeature : WebHostFeature<HealthCheckRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterHealthCheck(ConfigureAction);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseHealthCheck();
    }
}
