namespace Nameless.Text {

	/// <summary>
	/// Contract to data binder functionality.
	/// </summary>
	public interface IDataBinder {

        #region Methods

        /// <summary>
        /// Evals an expression, for a given container, and returns its result.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="format">A format pattern.</param>
        /// <returns>The result of the expression.</returns>
        object Eval(object container, string expression, string? format = null);

        #endregion Methods
    }
}
