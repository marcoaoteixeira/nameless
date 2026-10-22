using Microsoft.AspNetCore.Builder;
using Nameless.Web.Cors;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers CORS, configured through the configuration, and uses its middleware.
/// </summary>
public class CorsFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterCors(builder.Configuration);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseCors();
    }
}
