using Microsoft.AspNetCore.Builder;
using Nameless.Mediator;
using Nameless.Registration;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the mediator, scanning the context assemblies.
/// </summary>
public class MediatorFeature : WebHostFeature<MediatorRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterMediator(
            AssemblyScanAwareHelper.MergeAssemblies(ConfigureAction, context.Assemblies)
        );
    }
}
