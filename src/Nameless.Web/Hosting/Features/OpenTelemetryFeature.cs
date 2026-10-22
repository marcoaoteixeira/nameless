using Microsoft.AspNetCore.Builder;
using Nameless.Web.Observability;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers OpenTelemetry (tracing, metrics and logging).
/// </summary>
public class OpenTelemetryFeature : WebHostFeature<OpenTelemetryRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.RegisterOpenTelemetry(ConfigureAction);
    }
}
