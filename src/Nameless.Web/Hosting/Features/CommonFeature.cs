using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the services every web host needs: options, problem
///     details, the HTTP context accessor and <see cref="TimeProvider"/>.
/// </summary>
public class CommonFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.AddOptions();
        builder.Services.AddProblemDetails();
        builder.Services.AddHttpContextAccessor();

        // TryAdd so tests can replace it with a fake time provider.
        builder.Services.TryAddSingleton(TimeProvider.System);
    }
}
