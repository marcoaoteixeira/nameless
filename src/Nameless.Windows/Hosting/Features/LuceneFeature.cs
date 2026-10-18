using Nameless.Lucene;
using Nameless.Registration;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers Lucene, scanning the context assemblies.
/// </summary>
public class LuceneFeature : WinHostFeature<LuceneRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            (ctx, services) => services.RegisterLucene(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies),
                ctx.Configuration
            )
        );
    }
}
