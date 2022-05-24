namespace Nameless.FileStorage {

    /// <summary>
    /// Represents a generic abstraction over a file storage.
    /// </summary>
    /// /// <remarks>
    /// The virtual file storage uses forward slash (/) as the relativePath delimiter,
    /// and has no concept of volumes or drives. All relativePaths are specified and
    /// returned as relative to the root of the virtual file storage. Absolute
    /// relativePaths using a leading slash or leading period, and parent traversal
    /// using "../", are not supported.
    /// 
    /// This abstraction does not dictate any case sensitivity semantics. Case
    /// sensitivity is left to the underlying storage system of concrete
    /// implementations. For example, the Windows file system is case
    /// insensitive, while Linux file system and Azure Blob Storage are case
    /// sensitive.
    /// </remarks>
    public interface IFileStorage {

        #region Properties

        string Root { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a directory in the file storage if it doesn't already exist.
        /// </summary>
        /// <param name="relativePath">
        /// The relative path of the directory to be created.
        /// </param>
        /// <returns>
        /// <c>true</c> if the directory was created; <c>false</c> if the
        /// directory already existed.
        /// </returns>
        Task<bool> CreateDirectoryAsync(string relativePath, CancellationToken token = default);

        /// <summary>
        /// Retrieves information about the given directory within the file
        /// storage.
        /// </summary>
        /// <param name="relativePath">
        /// The relative path to the directory.
        /// </param>
        /// <returns>
        /// A <see cref="IDirectory"/> object representing the directory.
        /// </returns>
        Task<IDirectory> GetDirectoryAsync(string relativePath);

        /// <summary>
        /// Creates a new file in the file storage from the contents of an
        /// input stream.
        /// </summary>
        /// <param name="relativePath">
        /// The relative path of the file to be created.
        /// </param>
        /// <param name="input">
        /// The stream whose contents to write to the new file.
        /// </param>
        /// <param name="overwrite">
        /// <c>true</c> to overwrite if a file already exists; <c>false</c>
        /// to throw an exception if the file already exists.
        /// </param>
        /// <returns>
        /// A <see cref="Task" /> representing the method execution.
        /// </returns>
        Task CreateFileAsync(string relativePath, Stream input, bool overwrite = false, CancellationToken token = default);

        /// <summary>
        /// Retrieves information about the given file within the file store.
        /// </summary>
        /// <param name="relativePath">The relative path to the file.</param>
        /// <returns>
        /// A <see cref="IFile"/> object representing the file.
        /// </returns>
        Task<IFile> GetFileAsync(string relativePath);

        #endregion
    }
}