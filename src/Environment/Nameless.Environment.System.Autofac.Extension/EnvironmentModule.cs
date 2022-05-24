using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Environment.System {

    /// <summary>
    /// The Environment service registration.
    /// </summary>
    public sealed class EnvironmentModule : ModuleBase {

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.Register<IHostEnvironment, HostEnvironment>(lifetimeScope: LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }
}
