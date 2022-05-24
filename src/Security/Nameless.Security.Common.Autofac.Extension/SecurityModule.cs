using Autofac;
using Nameless.DependencyInjection.Autofac;
using Nameless.Security.Cryptography;

namespace Nameless.Security {

    public sealed class SecurityModule : ModuleBase {

        #region Public Enums

        public enum CryptographyAlgorithm {
            AES,

            Rijndael
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the encryption algorithm.
        /// </summary>
        /// <remarks>Default is <see cref="CryptographyAlgorithm.AES"/>.</remarks>
        public CryptographyAlgorithm Algorithm { get; set; } = CryptographyAlgorithm.AES;

        #endregion

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder
                .Register(
                    service: typeof(IPasswordGenerator),
                    implementation: typeof(RandomPasswordGenerator),
                    lifetimeScope: LifetimeScopeType.Singleton
                )
                .Register(
                    service: typeof(ICryptoProvider),
                    implementation: Algorithm == CryptographyAlgorithm.AES ? typeof(AesCryptoProvider) : typeof(RijndaelCryptoProvider),
                    lifetimeScope: Algorithm == CryptographyAlgorithm.AES ? LifetimeScopeType.Singleton : LifetimeScopeType.Transient
                );

            base.Load(builder);
        }

        #endregion
    }
}
