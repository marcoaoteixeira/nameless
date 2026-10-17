namespace Nameless.EntityFrameworkCore;

/// <summary>
///     Missing Database Seeder Resource Exception
/// </summary>
public class MissingDatabaseSeederResourceException : Exception {
    /// <summary>
    ///     Initializes a new instance of
    ///     <see cref="MissingDatabaseSeederResourceException"/> class.
    /// </summary>
    /// <param name="path">
    ///     The resource path.
    /// </param>
    public MissingDatabaseSeederResourceException(string path)
        : base($"Unable to locate database seeder resource '{path}'.", innerException: null) { }
}