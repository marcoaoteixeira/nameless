using Microsoft.Extensions.Hosting;
using Nameless.Registration;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.Navigation;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the navigation services, scanning the context assemblies.
/// </summary>
public class NavigationFeature : WinHostFeature<NavigationRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterNavigation(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
