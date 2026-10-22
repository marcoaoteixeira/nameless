using Microsoft.AspNetCore.Builder;
using Nameless.Reporting;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the status reporting services.
/// </summary>
public class StatusReportingFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterStatusReporting(builder.Configuration);
    }
}
