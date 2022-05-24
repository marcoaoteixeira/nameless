using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Serialization.Json {

    public sealed class SerializationModule : ModuleBase {

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder.Register<ISerializer, JsonSerializer>(lifetimeScope: LifetimeScopeType.Singleton);

            base.Load(builder);
        }

        #endregion
    }
}
