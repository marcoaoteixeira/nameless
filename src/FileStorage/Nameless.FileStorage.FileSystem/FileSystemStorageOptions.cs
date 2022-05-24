namespace Nameless.FileStorage.FileSystem {

	public sealed class FileSystemStorageOptions {

		#region Public Properties

		/// <summary>
		/// Gets or sets the root path of the file storage. Default value is
		/// the <see cref="FileSystemStorageOptions" /> assembly reside path, plus
		/// "App_Data" folder.
		/// </summary>
		public string Root { get; set; }

		#endregion
	}
}