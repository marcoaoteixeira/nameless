namespace Nameless.Localization.Json {

    public sealed class LocalizationOptions {

        #region Public Properties

        /// <summary>
        /// Gets or sets the localization resources folder. The folder path
        /// must be relative to the application file storage root path.
        /// </summary>
        public string ResourceFolderPath { get; set; } = "Localization";

        /// <summary>
        /// Gets or sets the default culture name.
        /// </summary>
        public string DefaultCultureName { get; set; } = "en-US";

        /// <summary>
        /// Whether it will watch for changes in localization resource files
        /// and reload it.
        /// </summary>
        public bool ReloadOnChange { get; set; } = true;

        #endregion
    }
}
