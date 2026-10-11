using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.Windows.Dialogs.FileSystem;

/// <summary>
///     <see cref="IServiceCollection"/> extensions methods.
/// </summary>
public static class FileSystemServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the <see cref="IFileSystemDialog"/> service.
        /// </summary>
        /// <returns>
        ///     The <see cref="IServiceCollection"/> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterFileSystemDialog() {
            self.TryAddSingleton<IFileSystemDialog, FileSystemDialog>();

            return self;
        }
    }
}
