using Nameless.Reporting;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the status reporting services.
/// </summary>
public class StatusReportingFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            (ctx, services) => services.RegisterStatusReporting(ctx.Configuration)
        );
    }
}
