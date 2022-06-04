using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Nameless.WebApplication.Web.Utils {

    public static class Password {

        #region Private Constant Fields

        private const int SALT_SIZE = 256;
        private const int ITERATION_COUNT = 100_000;

        #endregion

        #region Private Static Methods

        private static string InnerHash(string password, byte[]? salt = null) {
            salt ??= RandomNumberGenerator.GetBytes(SALT_SIZE);
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: ITERATION_COUNT,
                numBytesRequested: (SALT_SIZE * 2) / 8
            );
            // Make the output be salt and hash
            // array in this particular order.
            var output = Enumerable
                .Concat(salt, hash)
                .ToArray();

            return Convert.ToBase64String(output);
        }

        #endregion

        #region Public Static Methods

        public static string Hash(string password) {
            if (string.IsNullOrWhiteSpace(password)) { return string.Empty; }

            return InnerHash(password);
        }

        public static bool Validate(string password, string hash) {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash)) { return false; }

            // Here we'll break the password hash into the
            // salt and hash array, because it was stored
            // together in this particular order.
            var array = Convert.FromBase64String(hash);
            var salt = array[..SALT_SIZE];
            var currentHash = InnerHash(password, salt);

            return hash == currentHash;
        }

        #endregion
    }
}
