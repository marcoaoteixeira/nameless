using Microsoft.Extensions.Hosting;
using Nameless.Registration;
using Nameless.Windows.Bootstrap;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the bootstrap services, scanning the context assemblies.
/// </summary>
public class BootstrapFeature : WinHostFeature<BootstrapRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterBootstrap(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
