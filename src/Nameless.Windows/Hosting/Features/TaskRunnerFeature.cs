using Microsoft.Extensions.Hosting;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.TaskRunner;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the task runner.
/// </summary>
public class TaskRunnerFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterTaskRunner()
        );
    }
}
