using Microsoft.AspNetCore.Builder;
using Nameless.Web.Correlation;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the HTTP request correlation ID services and uses their middleware.
/// </summary>
public class HttpRequestCorrelationFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterHttpRequestCorrelation(builder.Configuration);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseHttpRequestCorrelation();
    }
}
