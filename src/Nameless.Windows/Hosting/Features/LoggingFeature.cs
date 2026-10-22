using Microsoft.Extensions.Hosting;
using Nameless.Logging.Serilog;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers Serilog.
/// </summary>
public class LoggingFeature : WinHostFeature<SerilogRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;

        builder.ConfigureServices(
            services => services.RegisterSerilog(configure)
        );
    }
}
