using Microsoft.Extensions.Hosting;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.Office;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the Office services.
/// </summary>
public class OfficeFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterOffice()
        );
    }
}
