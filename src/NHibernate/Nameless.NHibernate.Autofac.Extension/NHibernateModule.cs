using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Nameless.NHibernate {

    public sealed class NHibernateModule : ModuleBase {

        #region Private Constants

        private const string CONFIGURATION_BUILDER_KEY = "e7c9cd70-03fe-492b-8729-754e69a09575";
        private const string SESSION_PROVIDER_KEY = "b7300b0b-91cd-4180-a9e6-ed16a2e311a0";

        #endregion

        #region Private Static Methods

        private static void SessionProviderActivating(IActivatingEventArgs<SessionProvider> args) {
            var opts = args.Context.ResolveOptional<NHibernateOptions>() ?? new();
            if (!string.IsNullOrWhiteSpace(opts.SchemaOutputPath)) {
                var configurationBuilder = args.Context.ResolveNamed<IConfigurationBuilder>(CONFIGURATION_BUILDER_KEY);
                var configuration = configurationBuilder.Build(opts);
                
                using var session = args.Instance.GetSession();
                ExecuteSchemaExport(session, configuration, opts.SchemaOutputPath);
            }
        }

        private static void ExecuteSchemaExport(ISession session, Configuration configuration, string? schemaOutputPath) {
            var currentSchemaOutputPath = string.IsNullOrWhiteSpace(schemaOutputPath)
                ? Path.Combine(typeof(NHibernateModule).Assembly.GetDirectoryPath() ?? string.Empty, "schema.txt")
                : schemaOutputPath;

            using var writer = File.CreateText(currentSchemaOutputPath);
            writer.AutoFlush = true;
            new SchemaExport(configuration)
                .Execute(
                    useStdOut: false,
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
                .Register(ctx => ctx.ResolveNamed<ISessionProvider>(SESSION_PROVIDER_KEY).GetSession())
                .As<ISession>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }
}
