namespace Nameless.FileStorage {

    /// <summary>
    /// Represents an abstract directory in the file storage.
    /// </summary>
    public interface IDirectory {

        #region Properties

        /// <summary>
        /// Gets the name of the directory.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path of the directory within the file storage.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets whether the directory exists or not.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gets the date and time in UTC when the directory was last modified.
        /// </summary>
        DateTimeOffset LastWriteTimeUtc { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Enumerates the files inside the current directory.
        /// </summary>
        /// <param name="includeSubDirectories">
        /// A flag to indicate whether to get the files from just the top
        /// directory or from all sub-directories as well.
        /// </param>
        /// <returns>The list of files in the given directory.</returns>
        IAsyncEnumerable<IFile> GetFilesAsync(bool includeSubDirectories = false);

        /// <summary>
        /// Enumerates the directories below the current directory.
        /// </summary>
        /// <param name="includeSubDirectories">
        /// A flag to indicate whether to get the directories from just the top
        /// directory or from all sub-directories as well.
        /// </param>
        /// <returns>The list of files in the given directory.</returns>
        IAsyncEnumerable<IDirectory> GetDirectoriesAsync(bool includeSubDirectories = false);

        /// <summary>
        /// Creates a copy of the directory.
        /// </summary>
        /// <param name="destPath">
        /// The destination path of the directory to be created or overwritten.
        /// </param>
        /// <param name="overwrite">
        /// Whether it will overwrite the directory, if exists, or not.
        /// </param>
        /// <param name="token">The cancellation token.</param>
        Task CopyAsync(string destPath, bool overwrite = false, CancellationToken token = default);

        /// <summary>
        /// Moves the directory to another location or renames it.
        /// </summary>
        /// <param name="destPath">
        /// The destination path of the directory to be moved or renamed.
        /// </param>
        /// <param name="token">The cancellation token.</param>
        Task MoveAsync(string destPath, CancellationToken token = default);

        /// <summary>
        /// Deletes the directory, if it exists.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>
        /// <c>true</c> if the directory was deleted; <c>false</c> if not.
        /// </returns>
        Task<bool> DeleteAsync(CancellationToken token = default);

        /// <summary>
        /// Watchs for changes inside the current directory.
        /// </summary>
        /// <param name="callback">
        /// The callback that will be executed when something happens inside the
        /// diretory.
        /// </param>
        /// <param name="filter">The filter, if any; otherwise "*.*"</param>
        IDisposable Watch(Action<ChangeEventArgs> callback, string? filter = null);

        #endregion
    }
}