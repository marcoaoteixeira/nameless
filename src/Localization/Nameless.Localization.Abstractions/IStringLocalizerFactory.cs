namespace Nameless.Localization {

    public interface IStringLocalizerFactory {

        #region Methods

        /// <summary>
        /// Creates a <see cref="IStringLocalizer"/>, using the source type.
        /// </summary>
        /// <param name="source">The source type.</param>
        /// <returns>An instance of <see cref="IStringLocalizer"/> implementation.</returns>
        IStringLocalizer Create(Type source);
        /// <summary>
        /// Creates a <see cref="IStringLocalizer"/>, using the source name and source path.
        /// </summary>
        /// <param name="sourceName">The source name.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <returns>An instance of <see cref="IStringLocalizer"/> implementation.</returns>
        IStringLocalizer Create(string sourceName, string sourcePath);

        #endregion
    }
}