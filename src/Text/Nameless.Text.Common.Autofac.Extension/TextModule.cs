using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Text {

    public sealed class TextModule : ModuleBase {

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder
                .Register<IDataBinder, DataBinder>(
                    lifetimeScope: LifetimeScopeType.Singleton
                )
                .Register<IInterpolator, Interpolator>(
                    lifetimeScope: LifetimeScopeType.Singleton
                );

            base.Load(builder);
        }

        #endregion
    }
}
