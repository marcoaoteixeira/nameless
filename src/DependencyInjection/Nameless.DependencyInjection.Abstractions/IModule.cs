namespace Nameless.DependencyInjection {

    public interface IModule {

		#region Methods

		void Configure(IServiceComposer composer);

		#endregion
	}
}