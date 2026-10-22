using Microsoft.Extensions.Hosting;
using Nameless.IO.Physical;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the file provider.
/// </summary>
public class FileProviderFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterFileProvider()
        );
    }
}
