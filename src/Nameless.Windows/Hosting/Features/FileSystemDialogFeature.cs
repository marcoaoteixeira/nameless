using Microsoft.Extensions.Hosting;
using Nameless.Windows.Dialogs.FileSystem;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the file system dialog.
/// </summary>
public class FileSystemDialogFeature : WinHostFeature {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        builder.ConfigureServices(
            services => services.RegisterFileSystemDialog()
        );
    }
}
