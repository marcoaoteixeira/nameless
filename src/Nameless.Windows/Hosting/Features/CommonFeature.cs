using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Application;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the services every Windows host needs: options,
///     <see cref="TimeProvider"/> and the application context.
/// </summary>
public class CommonFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices((ctx, services) => {
            services.AddOptions();
            services.TryAddSingleton(TimeProvider.System);
            services.RegisterApplicationContext(ctx.Configuration);
        });
    }
}
