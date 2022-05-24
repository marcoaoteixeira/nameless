namespace Nameless.Security.Cryptography {

    public interface ICryptoProvider {

        #region Methods

        /// <summary>
        /// Encrypts a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>.</param>
        /// <param name="options">The crypto options.</params>
        /// <returns>Returns an array of <see cref="byte"/>, that is the encrypted version of the <paramref name="stream"/>.</returns>
        byte[]? Encrypt(Stream? stream, CryptoOptions? options = null);

        /// <summary>
        /// Decrypts a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/>.</param>
        /// <param name="options">The crypto options.</params>
        /// <returns>Returns an array of <see cref="byte"/>, that is the decrypted version of the <paramref name="stream"/>.</returns>
        byte[]? Decrypt(Stream? stream, CryptoOptions? options = null);

        #endregion
    }
}
