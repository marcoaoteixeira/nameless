namespace Nameless.Text {

	/// <summary>
	/// Interpolation contract.
	/// </summary>
	public interface IInterpolator {

        #region Methods

        /// <summary>
        /// Interpolates a <see cref="string"/> with the data of <paramref name="obj"/>.
        /// </summary>
        /// <param name="message">The message to interpolate.</param>
        /// <param name="obj">The object data.</param>
        /// <returns>The interpolated result.</returns>
        string Interpolate(string message, object obj);

        #endregion
    }
}
