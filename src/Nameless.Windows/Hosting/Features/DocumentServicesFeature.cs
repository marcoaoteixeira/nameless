using Microsoft.Extensions.Hosting;
using Nameless.Registration;
using Nameless.Windows.Documents;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the document services, scanning the context assemblies.
/// </summary>
public class DocumentServicesFeature : WinHostFeature<DocumentServicesRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterDocumentServices(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
