using Microsoft.AspNetCore.Builder;
using Nameless.Logging.Serilog;
using Nameless.Web.Serilog;
using Serilog;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers Serilog, enriched with the request correlation ID, and
///     uses the Serilog request logging middleware.
/// </summary>
public class LoggingFeature : WebHostFeature<SerilogRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        // Captured now, like every other feature, so a later Configure call
        // has no effect.
        var configure = ConfigureAction;

        builder.Services.RegisterSerilog(registration => {
            ConfigureDefaults(registration);
            configure?.Invoke(registration);
        });
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseSerilogRequestLogging();
    }

    private static void ConfigureDefaults(SerilogRegistration registration) {
        registration.WithEnrichmentConfiguration((provider, enrich) => {
            // Enrich with request CorrelationId
            enrich.WithCorrelationId(provider);
        });
    }
}
