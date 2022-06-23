using System.Text;

namespace Nameless.Security.Cryptography {

    public static class CryptoProviderExtension {

        #region Private Static Methods

        private static string? ExecuteAction(ICryptoProvider? cryptoProvider, string? value, CryptoOptions? options, bool encrypt = true) {
            if (cryptoProvider == null) { throw new ArgumentNullException(nameof(cryptoProvider)); }
            if (value == null) { return default; }

            var opts = options ?? new();

            var array = opts.Encoding.GetBytes(value);
            using var stream = new MemoryStream(array);
            var result = encrypt
                ? cryptoProvider.Encrypt(stream, options)
                : cryptoProvider.Decrypt(stream, options);

            if (result == null) { return default; }

            return opts.Encoding.GetString(result);
        }

        #endregion

        #region Public Static Methods

        public static string? Encrypt(this ICryptoProvider? self, string? value, CryptoOptions? options = null) => ExecuteAction(self, value, options, true);

        public static string? Decrypt(this ICryptoProvider? self, string? value, CryptoOptions? options = null) => ExecuteAction(self, value, options, false);

        #endregion
    }
}
