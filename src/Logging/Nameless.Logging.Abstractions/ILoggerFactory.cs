namespace Nameless.Logging {

    /// <summary>
    /// Defines the factory for <see cref="ILogger"/> implementation instances.
    /// </summary>
    public interface ILoggerFactory {

		#region Methods

		/// <summary>
		/// Creates a new instance of the <see cref="ILogger"/> implementation for the given
		/// type.
		/// </summary>
		/// <param name="type">The type associated to the <see cref="ILogger"/>implementation.</param>
		/// <returns>An instance of <see cref="ILogger"/> implementation.</returns>
		ILogger CreateLogger(Type type);

		#endregion Methods
	}
}