using System.Text;

namespace Nameless.Security.Cryptography {

    public static class CryptoProviderExtension {

        #region Public Static Methods

        public static string? Encrypt(this ICryptoProvider? self, Stream? stream, CryptoOptions? options = null, Encoding? encoding = null) {
            if (self == null || stream == null) { return default; }

            var value = self.Encrypt(stream, options);

            if (value == null) { return default; }

            return (encoding ?? Encoding.ASCII).GetString(value);
        }

        public static string? Decrypt(this ICryptoProvider? self, Stream? stream, CryptoOptions? options = null, Encoding? encoding = null) {
            if (self == null || stream == null) { return default; }

            var value = self.Decrypt(stream, options);

            if (value == null) { return default; }

            return (encoding ?? Encoding.ASCII).GetString(value);
        }

        #endregion
    }
}
