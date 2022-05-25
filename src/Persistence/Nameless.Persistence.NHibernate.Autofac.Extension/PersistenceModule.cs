using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Persistence.NHibernate {

    public sealed class PersistenceModule : ModuleBase {

        #region Private Constants

        private const string PERSISTER_KEY = "aa09d5b5-15df-4fae-ba03-299a809fcbea";
        private const string QUERIER_KEY = "e851703c-d8d1-4f62-8ca2-1b6149951872";

        #endregion

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {

            builder
                .RegisterType<Writer>()
                .Named<IWriter>(PERSISTER_KEY)
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder.RegisterType<Reader>()
                .Named<IReader>(QUERIER_KEY)
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder
                .RegisterType<Repository>()
                .As<IRepository>()
                .WithParameters(new[] {
                    ResolvedParameter.ForNamed<IWriter>(PERSISTER_KEY),
                    ResolvedParameter.ForNamed<IReader>(QUERIER_KEY)
                })
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }
}
