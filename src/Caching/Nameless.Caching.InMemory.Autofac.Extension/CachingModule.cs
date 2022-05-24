using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Caching.InMemory {

    public sealed class CachingModule : ModuleBase {

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder.Register<ICache, InMemoryCache>(lifetimeScope: LifetimeScopeType.Singleton);

            base.Load(builder);
        }

        #endregion
    }
}
