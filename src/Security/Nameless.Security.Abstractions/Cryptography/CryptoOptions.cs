using System.Text;

namespace Nameless.Security.Cryptography {

    public class CryptoOptions {

        #region Public Properties

        /// <summary>
        /// Gets or sets the encryption/decryption key. It has a default value.
        /// </summary>
        public string Key { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        /// <summary>
        /// Gets or sets the encoding. Default value is <see cref="Encoding.UTF8" />.
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        #endregion
    }
}
