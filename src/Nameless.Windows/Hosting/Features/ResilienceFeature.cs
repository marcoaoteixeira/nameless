using Microsoft.Extensions.Hosting;
using Nameless.Resilience;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the resilience services.
/// </summary>
public class ResilienceFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterResilience()
        );
    }
}
