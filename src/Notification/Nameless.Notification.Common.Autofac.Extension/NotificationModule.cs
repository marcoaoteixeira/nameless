using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Notification {

    public sealed class NotificationModule : ModuleBase {

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder.Register<INotifier, Notifier>(lifetimeScope: LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }
}
