using Microsoft.AspNetCore.Builder;
using Nameless.Resilience;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the resilience services.
/// </summary>
public class ResilienceFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterResilience();
    }
}
