namespace Nameless.Text {

	/// <summary>
	/// Base class used in <see cref="Interpolator"/>.
	/// </summary>
	public abstract class TextExpression {

        #region Public Abstract Methods

        /// <summary>
        /// Evals the parameter object.
        /// </summary>
        /// <param name="obj">The object to use.</param>
        /// <returns>A <see cref="string"/> representation.</returns>
        public abstract string? Eval(object? obj);

        #endregion
    }
}
