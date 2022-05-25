namespace Nameless.FileStorage {

    /// <summary>
    /// Represents an abstract file in a virtual file storage.
    /// </summary>
    public interface IFile {

        #region Properties

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path of the file within the file storage.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the directory path where the file resides.
        /// </summary>
        string DirectoryPath { get; }

        /// <summary>
        /// Gets whether the file exists or not.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        long Length { get; }

        /// <summary>
        /// Gets the date and time in UTC when the file was last modified.
        /// </summary>
        DateTimeOffset LastWriteTimeUtc { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Opens a stream to read (only) the contents of a file.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>
        /// An instance of <see cref="Stream"/> that can be used to read (only)
        /// the contents of the file. The caller must close the stream when
        /// finished.
        /// </returns>
        Task<Stream> OpenAsync(CancellationToken token = default);

        /// <summary>
        /// Creates a copy of the file.
        /// </summary>
        /// <param name="destPath">
        /// The destination path of the file to be created or overwritten.
        /// </param>
        /// <param name="overwrite">
        /// Whether it will overwrite the file, if exists, or not.
        /// </param>
        /// <param name="token">The cancellation token.</param>
        Task CopyAsync(string destPath, bool overwrite = false, CancellationToken token = default);

        /// <summary>
        /// Moves the file to another location or renames it.
        /// </summary>
        /// <param name="destPath">
        /// The destination path of the file to be moved or renamed.
        /// </param>
        /// <param name="token">The cancellation token.</param>
        Task MoveAsync(string destPath, CancellationToken token = default);

        /// <summary>
        /// Deletes the file, if it exists.
        /// </summary>
        /// <param name="token">The cancellation token.</param>
        /// <returns>
        /// <c>true</c> if the file was deleted; <c>false</c> if not.
        /// </returns>
        Task<bool> DeleteAsync(CancellationToken token = default);

        /// <summary>
        /// Watchs for changes to the file.
        /// </summary>
        /// <param name="callback">The change callback.</param>
        IDisposable Watch(Action<ChangeEventArgs> callback);

        #endregion
    }
}