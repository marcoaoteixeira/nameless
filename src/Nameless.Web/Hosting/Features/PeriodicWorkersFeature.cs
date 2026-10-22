using Microsoft.AspNetCore.Builder;
using Nameless.Registration;
using Nameless.Workers;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the periodic workers, scanning the context assemblies.
/// </summary>
public class PeriodicWorkersFeature : WebHostFeature<PeriodicWorkersRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterPeriodicWorkers(
            AssemblyScanAwareHelper.MergeAssemblies(ConfigureAction, context.Assemblies)
        );
    }
}
