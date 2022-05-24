using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Nameless.WebApplication.Web.Utils {

    internal static class PasswordUtils {

        #region Internal Static Methods

        internal static string HashPassword(string? password, int size = 128) {
            if (string.IsNullOrWhiteSpace(password)) { return string.Empty; }

            var salt = RandomNumberGenerator.GetBytes(size);
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: (size * 2) / 8
            );

            return Convert.ToBase64String(hash);
        }

        #endregion
    }
}
