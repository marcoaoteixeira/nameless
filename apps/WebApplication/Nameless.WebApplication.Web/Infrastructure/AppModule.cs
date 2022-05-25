using Autofac;
using Nameless.DependencyInjection.Autofac;
using Nameless.WebApplication.Web.Persistence;

namespace Nameless.WebApplication.Web.Infrastructure {

    public sealed class AppModule : ModuleBase {

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {
            builder
                .RegisterType<Repository>()
                .As<IRepository>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }
}
