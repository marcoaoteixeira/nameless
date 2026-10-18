using Microsoft.Extensions.Hosting;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the content dialog service.
/// </summary>
public class ContentDialogServiceFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterContentDialogService()
        );
    }
}
