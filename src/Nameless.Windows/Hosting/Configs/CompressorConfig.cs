using Microsoft.Extensions.Hosting;
using Nameless.Compression;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Configs;

/// <summary>
///     Compressor Configuration
/// </summary>
public static class CompressorConfig {
    /// <param name="self">
    ///     The current <see cref="WinHostFactory"/> instance.
    /// </param>
    extension(WinHostBuilder self) {
        /// <summary>
        ///     Configures the Compressor service.
        /// </summary>
        /// <param name="settings">
        ///     The <see cref="WinHostSettings"/> instance.
        /// </param>
        /// <returns>
        ///     The current <see cref="WinHostFactory"/> instance so other
        ///     actions can be chained.
        /// </returns>
        public WinHostBuilder ConfigureZipCompressorFeature(WinHostSettings settings) {
            if (settings.DisableCompressor) { return self; }

            self.ConfigureServices(
                services => services.RegisterZipCompressor()
            );

            return self;
        }
    }
}