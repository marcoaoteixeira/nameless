using Microsoft.Extensions.Hosting;
using Nameless.Registration;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.UI;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the window factory, scanning the context assemblies.
/// </summary>
public class WindowFactoryFeature : WinHostFeature<WindowFactoryRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterWindowFactory(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
