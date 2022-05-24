namespace Nameless.DependencyInjection {

    /// <summary>
    /// Service composer interface.
    /// </summary>
    public interface IServiceComposer {

		#region Methods

		IServiceComposer AddTransient(Type serviceType, Type implementationType, params Parameter[] parameters);
		IServiceComposer AddPerScope(Type serviceType, Type implementationType, params Parameter[] parameters);
		IServiceComposer AddSingleton(Type serviceType, Type implementationType, params Parameter[] parameters);
		IServiceComposer AddModule(Type moduleType);
		IServiceComposer AddModule(IModule module);

		#endregion Methods
	}
}