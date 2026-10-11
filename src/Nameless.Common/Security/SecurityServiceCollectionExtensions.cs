using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Security.Cryptography;
using Nameless.Security.Password;

namespace Nameless.Security;

/// <summary>
///     <see cref="IServiceCollection"/> extensions for security features.
/// </summary>
public static class SecurityServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/> instance.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers security services.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterSecurity(IConfiguration? configuration = null) {
            self.ConfigureOptions<RijndaelCryptoOptions>(configuration);

            self.TryAddSingleton<IPassGenerator, RandomPassGenerator>();
            self.TryAddSingleton<ICrypto, RijndaelCrypto>();

            return self;
        }
    }
}