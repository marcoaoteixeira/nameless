using Microsoft.AspNetCore.Builder;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Uses the endpoint routing middleware.
/// </summary>
public class EndpointRoutingFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseRouting();
    }
}
