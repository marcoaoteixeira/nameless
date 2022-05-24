namespace Nameless.DependencyInjection {

	/// <summary>
	/// Extension methods for <see cref="IServiceResolver"/>.
	/// </summary>
	public static class ServiceResolverExtension {

		#region Public Static Methods

		/// <summary>
		/// Resolves a service by its type.
		/// </summary>
		/// <typeparam name="TService">The type of the service.</typeparam>
		/// <param name="self">The implemented <see cref="IServiceResolver"/> instance.</param>
		/// <param name="name">The name of the service, if any.</param>
		/// <param name="parameters">The resolver parameters.</param>
		/// <returns>An instance of the service.</returns>
		public static TService? Get<TService>(this IServiceResolver self, string? name = null, params Parameter[] parameters) {
			if (self == null) { throw new ArgumentNullException(nameof(self)); }

			return (TService)self.Get(serviceType: typeof(TService), name: name, parameters: parameters);
		}

		#endregion Public Static Methods
	}
}