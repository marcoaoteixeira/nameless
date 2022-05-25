namespace Nameless.FileStorage.System {

    public static class PathHelper {

        #region Public Static Methods

        /// <summary>
        /// Normalizes a path using the path delimiter semantics of the
        /// underlying OS platform.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On Windows: Foward slash is converted to backslash and any leading
        /// or trailing slashes are removed.
        /// </para>
        /// <para>
        /// On Linux and OSX: Backslash is converted to foward slash and any
        /// leading or trailing slashes are removed.
        /// </para>
        /// </remarks>
        public static string Normalize(string? path) {
            Prevent.NullEmptyOrWhiteSpace(path, nameof(path));

            var result = path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .Trim(Path.DirectorySeparatorChar);

            return result;
        }

        /// <summary>
        /// Retrieves the physical path to a file.
        /// It also executes the <see cref="Normalize(string)"/> method.
        /// </summary>
        /// <param name="root">The root path.</param>
        /// <param name="relativePath">
        /// The relative path to the <paramref name="root" />.
        /// </param>
        /// <param name="allowOutsideFileSystem">
        /// <c>true</c> if allow search outside the root; otherwise
        /// <c>false</c>.
        /// </param>
        /// <returns>The physical path to the content.</returns>
        public static string GetPhysicalPath(string root, string relativePath) {
            Prevent.NullEmptyOrWhiteSpace(root, nameof(root));
            Prevent.NullEmptyOrWhiteSpace(relativePath, nameof(relativePath));

            var currentRoot = Normalize(root);

            // Assert root path
            currentRoot = currentRoot.EndsWith(Path.DirectorySeparatorChar)
                ? currentRoot
                : string.Concat(currentRoot, Path.DirectorySeparatorChar);

            var currentRelativePath = Normalize(relativePath);
            var result = Path.Combine(currentRoot, currentRelativePath);

            // Verify that the resulting path is inside the root file system path.
            var isInsideFileSystem = Path.GetFullPath(result).StartsWith(currentRoot, StringComparison.OrdinalIgnoreCase);
            if (!isInsideFileSystem) {
                throw new PathResolutionException($"The path '{currentRelativePath}' resolves to a physical path outside the file storage root.");
            }

            return result;
        }

        #endregion
    }
}