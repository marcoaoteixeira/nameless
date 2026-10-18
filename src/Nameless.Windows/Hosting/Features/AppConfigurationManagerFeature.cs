using Microsoft.Extensions.Hosting;
using Nameless.Windows.Configuration;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the application configuration manager.
/// </summary>
public class AppConfigurationManagerFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterAppConfigurationManager()
        );
    }
}
