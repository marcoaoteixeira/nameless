using System.Reflection;

namespace Nameless {

    /// <summary>
    /// Assembly object extension methods.
    /// </summary>
    public static class AssemblyExtension {

        #region Public Static Methods

        /// <summary>
        /// Retrieves the assembly directory path.
        /// </summary>
        /// <param name="self">The current assembly.</param>
        /// <returns>The path to the assembly folder.</returns>
        public static string? GetDirectoryPath(this Assembly self) {
            if (self == null) { return default; }

            var location = OperatingSystem.IsWindows() ? self.Location : $"file://{self.Location}";
            var uri = new UriBuilder(location);
            var path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        #endregion
    }
}