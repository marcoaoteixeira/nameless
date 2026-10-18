using Microsoft.Extensions.Hosting;
using Nameless.Windows.Dialogs.Message;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the message dialog.
/// </summary>
public class MessageDialogFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterMessageDialog()
        );
    }
}
