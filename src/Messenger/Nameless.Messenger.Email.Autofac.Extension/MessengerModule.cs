using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Messenger.Email {

	public sealed class MessengerModule : ModuleBase {

		#region Protected Override Methods

		protected override void Load(ContainerBuilder builder) {
			builder
				.Register(
					service: typeof(IMessenger),
					implementation: typeof(Messenger),
					lifetimeScope: LifetimeScopeType.Singleton
				);

			base.Load(builder);
		}

		#endregion
	}
}
