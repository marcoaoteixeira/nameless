using Microsoft.Extensions.Hosting;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.Messaging;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the messenger.
/// </summary>
public class MessengerFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterMessenger()
        );
    }
}
