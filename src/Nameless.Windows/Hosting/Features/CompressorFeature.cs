using Microsoft.Extensions.Hosting;
using Nameless.Compression;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the ZIP compressor.
/// </summary>
public class CompressorFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterZipCompressor()
        );
    }
}
