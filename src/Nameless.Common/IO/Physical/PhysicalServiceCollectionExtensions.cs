using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.IO.Physical;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods for File Explorer Provider.
/// </summary>
public static class PhysicalServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/> instance.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the File Explorer Provider that creates instances of
        ///     <see cref="IFileProvider"/>.
        /// </summary>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> instance so other
        ///     actions can be chained.
        /// </returns>
        public IServiceCollection RegisterFileProvider() {
            self.TryAddSingleton<FileProviderFactory>();
            self.TryAddSingleton<IFileProvider>(provider => provider.GetRequiredService<FileProviderFactory>()
                                                                    .GetOrCreate(AppContext.BaseDirectory));

            return self;
        }
    }
}
