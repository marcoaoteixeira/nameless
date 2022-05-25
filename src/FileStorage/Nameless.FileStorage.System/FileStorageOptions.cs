namespace Nameless.FileStorage.System {

    public sealed class FileStorageOptions {

        #region Public Static Read-Only Methods

        public static readonly FileStorageOptions Default = new();

        #endregion


        #region Public Properties

        /// <summary>
        /// Gets or sets the root path of the file storage. Default value is
        /// the <see cref="FileStorageOptions" /> assembly reside path, plus
        /// "App_Data" folder.
        /// </summary>
        public string Root { get; set; } = Path.Combine(typeof(FileStorageOptions).Assembly.GetDirectoryPath(), "App_Data");

        #endregion
    }
}