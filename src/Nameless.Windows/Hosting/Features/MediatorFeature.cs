using Microsoft.Extensions.Hosting;
using Nameless.Mediator;
using Nameless.Registration;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the mediator, scanning the context assemblies.
/// </summary>
public class MediatorFeature : WinHostFeature<MediatorRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterMediator(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
