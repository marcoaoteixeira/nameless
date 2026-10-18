using Microsoft.Extensions.Hosting;
using Nameless.Registration;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.Mvvm;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the view models, scanning the context assemblies.
/// </summary>
public class ViewModelsFeature : WinHostFeature<ViewModelRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterViewModels(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
