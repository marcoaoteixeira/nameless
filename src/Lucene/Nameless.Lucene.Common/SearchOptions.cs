using System.IO;

namespace Nameless.Lucene {

	/// <summary>
	/// Lucene Search Settings.
	/// </summary>
	public sealed class SearchOptions {

        #region Public Properties

        /// <summary>
        /// Gets or sets the index storage directory path, relative to the
        /// application. Default value is "./App_Data/Search/Lucene".
        /// </summary>
        public string IndexStorageDirectoryPath { get; set; } = Path.Combine("App_Data", "Search", "Lucene");

        #endregion
    }
}
