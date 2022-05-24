namespace Nameless.Security {

    /// <summary>
    /// Defines methods to generate passwords.
    /// </summary>
    public interface IPasswordGenerator {

        #region	Methods

        /// <summary>
        /// Generates a password with the given parameters.
        /// </summary>
        /// <param name="opts">Password generator options</param>
        /// <returns>The <see cref="string"/> representation of the generated password.</returns>
        string? Generate(PasswordGeneratorOptions? opts = null);

        #endregion
    }
}
