using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;
using Nameless.NHibernate;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Nameless.Persistence.NHibernate {

    public sealed class PersistenceModule : ModuleBase {

        #region Private Constants

        private const string CONFIGURATION_BUILDER_KEY = "bc47d07b-5f1c-475c-800e-aeb02f4d71ca";
        private const string SESSION_PROVIDER_KEY = "c3bd9315-18cf-4585-98a8-ce71fb6e9372";
        private const string SESSION_KEY = "13a5a03b-04df-4b83-a6b7-78b3f1d5743d";
        private const string PERSISTER_KEY = "aa09d5b5-15df-4fae-ba03-299a809fcbea";
        private const string QUERIER_KEY = "e851703c-d8d1-4f62-8ca2-1b6149951872";
        private const string DIRECTIVE_EXECUTOR_KEY = "32146fac-42a3-4199-8fba-56c6270776eb";

        #endregion

        #region Private Static Methods

        private static void SessionProviderActivating(IActivatingEventArgs<SessionProvider> args) {
            var opts = args.Context.ResolveOptional<NHibernateOptions>() ?? new();
            if (opts.ExecuteSchemaExport) {
                var configurationBuilder = args.Context.ResolveNamed<IConfigurationBuilder>(CONFIGURATION_BUILDER_KEY);
                var configuration = configurationBuilder.Build(opts);

                using var session = args.Instance.GetSession();
                ExecuteSchemaExport(session, configuration, opts.SchemaOutputPath);
            }
        }

        private static void ExecuteSchemaExport(ISession session, Configuration configuration, string? schemaOutputPath) {
            var currentSchemaOutputPath = string.IsNullOrWhiteSpace(schemaOutputPath)
                ? Path.Combine(typeof(PersistenceModule).Assembly.GetDirectoryPath() ?? string.Empty, "schema.txt")
                : schemaOutputPath;

            using var writer = File.CreateText(currentSchemaOutputPath);
            writer.AutoFlush = true;
            new SchemaExport(configuration)
                .Execute(
                    useStdOut: true,
                    execute: false,
                    justDrop: false,
                    connection: session.Connection,
                    exportOutput: writer
                );
        }

        #endregion

        #region Protected Override Methods

        protected override void Load(ContainerBuilder builder) {

            builder
                .RegisterType<ConfigurationBuilder>()
                .Named<IConfigurationBuilder>(CONFIGURATION_BUILDER_KEY)
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .RegisterType<SessionProvider>()
                .Named<ISessionProvider>(SESSION_PROVIDER_KEY)
                .WithParameter(ResolvedParameter.ForNamed<IConfigurationBuilder>(CONFIGURATION_BUILDER_KEY))
                .OnActivating(SessionProviderActivating)
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .Register(ctx => ctx.ResolveNamed<ISessionProvider>(SESSION_PROVIDER_KEY))
                .Named<ISession>(SESSION_KEY)
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder
                .RegisterType<Writer>()
                .Named<IWriter>(PERSISTER_KEY)
                .WithParameter(ResolvedParameter.ForNamed<ISession>(SESSION_KEY))
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder.RegisterType<Reader>()
                .Named<IReader>(QUERIER_KEY)
                .WithParameter(ResolvedParameter.ForNamed<ISession>(SESSION_KEY))
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder.RegisterType<DirectiveExecutor>()
                .Named<IDirectiveExecutor>(DIRECTIVE_EXECUTOR_KEY)
                .WithParameter(ResolvedParameter.ForNamed<ISession>(SESSION_KEY))
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            builder
                .RegisterType<Repository>()
                .As<IRepository>()
                .WithParameters(new[] {
                    ResolvedParameter.ForNamed<IDirectiveExecutor>(DIRECTIVE_EXECUTOR_KEY),
                    ResolvedParameter.ForNamed<IWriter>(PERSISTER_KEY),
                    ResolvedParameter.ForNamed<IReader>(QUERIER_KEY)
                })
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }
}
