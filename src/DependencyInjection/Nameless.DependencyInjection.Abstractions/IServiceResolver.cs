namespace Nameless.DependencyInjection {

    /// <summary>
    /// Resolver interface.
    /// </summary>
    public interface IServiceResolver : IDisposable {

		#region Properties

		bool IsRoot { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Resolves a service by its type.
		/// </summary>
		/// <param name="serviceType">The service type.</param>
		/// <param name="name">The service name. If any.</param>
		/// <param name="parameters">A collection of parameters.</param>
		/// <returns>The instance of the service.</returns>
		object Get(Type serviceType, string? name = null, params Parameter[] parameters);

		/// <summary>
		/// Resolves a service by its type, if found.
		/// </summary>
		/// <param name="serviceType">The service type.</param>
		/// <param name="name">The service name. If any.</param>
		/// <param name="parameters">A collection of parameters.</param>
		/// <returns>The instance of the service or <c>null</c> if not found.</returns>
		object? GetOptional(Type serviceType, string? name = null, params Parameter[] parameters);

		/// <summary>
		/// Retrieves an <see cref="IServiceResolver" /> scope to this instance.
		/// </summary>
		/// <returns>A scoped instance of the current of <see cref="IServiceResolver" />.</returns>
		IServiceResolver GetScoped();

		/// <summary>
		/// Retrieves an <see cref="IServiceResolver" /> scope to this instance.
		/// </summary>
		/// <param name="composer">The service composer action.</param>
		/// <returns>A scoped instance of the current of <see cref="IServiceResolver" />.</returns>
		IServiceResolver GetScoped(Action<IServiceComposer> composer);

		#endregion Methods
	}
}