using Microsoft.AspNetCore.Builder;
using Nameless.Web.RequestTimeout;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers request timeouts, configured through the configuration, and uses their middleware.
/// </summary>
public class RequestTimeoutFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterRequestTimeout(builder.Configuration);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseRequestTimeouts();
    }
}
