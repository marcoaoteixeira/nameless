using Nameless.Registration;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.Localization;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers localization, scanning the context assemblies.
/// </summary>
public class LocalizationFeature : WinHostFeature<LocalizationRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            (ctx, services) => services.RegisterLocalization(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies),
                ctx.Configuration
            )
        );
    }
}
