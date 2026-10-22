using Microsoft.AspNetCore.Builder;
using Nameless.Web.OutputCache;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers output cache, configured through the configuration, and uses its middleware.
/// </summary>
public class OutputCacheFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterOutputCache(builder.Configuration);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseOutputCache();
    }
}
