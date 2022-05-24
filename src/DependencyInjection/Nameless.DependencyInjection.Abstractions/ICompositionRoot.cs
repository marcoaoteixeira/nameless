namespace Nameless.DependencyInjection {

    /// <summary>
    /// Composition root interface.
    /// </summary>
    public interface ICompositionRoot {

		#region Methods

		/// <summary>
		/// Composes the root.
		/// </summary>
		/// <param name="action">The service composer action.</param>
		void Compose(Action<IServiceComposer> action);

		/// <summary>
		/// Start up the composition root.
		/// </summary>
		void StartUp();

		/// <summary>
		/// Tears down the composition root.
		/// </summary>
		void TearDown();

		#endregion Methods
	}
}