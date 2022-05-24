namespace Nameless.Lucene {

    /// <summary>
    /// Defines methods for a document index.
    /// </summary>
    public interface IDocument {

		#region Methods

		/// <summary>
		/// Sets the document ID.
		/// </summary>
		/// <param name="id">Document ID.</param>
		/// <returns>The current instance of <see cref="IDocument"/>.</returns>
		IDocument SetID(string id);

		/// <summary>
		/// Adds a new <see cref="string"/> value to the document.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">The value of the field.</param>
		/// <param name="options">The field options.</param>
		/// <returns>The current instance of <see cref="IDocument"/>.</returns>
		IDocument Set(string fieldName, string value, DocumentOptions options = DocumentOptions.None);

		/// <summary>
		/// Adds a new <see cref="DateTimeOffset"/> value to the document.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">The value of the field.</param>
		/// <param name="options">The field options.</param>
		/// <returns>The current instance of <see cref="IDocument"/>.</returns>
		IDocument Set(string fieldName, DateTimeOffset value, DocumentOptions options = DocumentOptions.None);

		/// <summary>
		/// Adds a new <see cref="int"/> value to the document.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">The value of the field.</param>
		/// <param name="options">The field options.</param>
		/// <returns>The current instance of <see cref="IDocument"/>.</returns>
		IDocument Set(string fieldName, int value, DocumentOptions options = DocumentOptions.None);

		/// <summary>
		/// Adds a new <see cref="bool"/> value to the document.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">The value of the field.</param>
		/// <param name="options">The field options.</param>
		/// <returns>The current instance of <see cref="IDocument"/>.</returns>
		IDocument Set(string fieldName, bool value, DocumentOptions options = DocumentOptions.None);

		/// <summary>
		/// Adds a new <see cref="double"/> value to the document.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">The value of the field.</param>
		/// <param name="options">The field options.</param>
		/// <returns>The current instance of <see cref="IDocument"/>.</returns>
		IDocument Set(string fieldName, double value, DocumentOptions options = DocumentOptions.None);

		#endregion
	}
}
