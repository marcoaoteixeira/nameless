using Microsoft.AspNetCore.Builder;
using Nameless.Web.ServiceDiscovery;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers service discovery (Aspire).
/// </summary>
public class ServiceDiscoveryFeature : WebHostFeature<ServiceDiscoveryRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterServiceDiscovery(ConfigureAction);
    }
}
