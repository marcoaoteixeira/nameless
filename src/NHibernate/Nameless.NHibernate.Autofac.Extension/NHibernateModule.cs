using Autofac;
using Nameless.DependencyInjection.Autofac;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Nameless.NHibernate {

    public sealed class NHibernateModule : ModuleBase {

        #region Private Constants

        private const string CONFIGURATION_BUILDER_KEY = "e7c9cd70-03fe-492b-8729-754e69a09575";
        private const string CONFIGURATION_KEY = "01adb905-5589-4c2f-acd9-b14b6368589d";
        private const string SESSION_FACTORY_KEY = "f3434c42-a8e8-458c-bfff-5ee00aacecad";

        #endregion

        #region Public Properties

        public ExecuteSchemaInfo SchemaInfo { get; set; } = ExecuteSchemaInfo.Default;

        #endregion

        #region Private Static Methods

        private static Configuration ResolveConfiguration(IComponentContext ctx) {
            var options = ctx.ResolveOptional<NHibernateOptions>() ?? new();
            var configurationBuilder = ctx.ResolveNamed<IConfigurationBuilder>(CONFIGURATION_BUILDER_KEY);
            var result = configurationBuilder.Build(options);

            return result;
        }

        private static ISessionFactory ResolveSessionFactory(IComponentContext ctx, ExecuteSchemaInfo schemaInfo) {
            var configuration = ctx.ResolveNamed<Configuration>(CONFIGURATION_KEY);
            var sessionFactory = configuration.BuildSessionFactory();
            if (schemaInfo.ExecuteSchema == ExecuteSchemaOptions.OnSessionFactoryResolution) {
                using var session = sessionFactory.OpenSession();

                ExecuteSchemaExport(session, configuration, schemaInfo);
            }

            return sessionFactory;
        }

        private static ISession ResolveSession(IComponentContext ctx, ExecuteSchemaInfo schemaInfo) {
            var sessionFactory = ctx.ResolveNamed<ISessionFactory>(SESSION_FACTORY_KEY);
            var session = sessionFactory.OpenSession();

            if (schemaInfo.ExecuteSchema == ExecuteSchemaOptions.OnSessionResolution) {
                var configuration = ctx.ResolveNamed<Configuration>(CONFIGURATION_KEY);

                ExecuteSchemaExport(session, configuration, schemaInfo);
            }

            return session;
        }

        private static void ExecuteSchemaExport(ISession session, Configuration configuration, ExecuteSchemaInfo schemaInfo) {
            var outputConsole = !schemaInfo.SchemaOutput.HasFlag(SchemaOutputOptions.None) &&
                                 schemaInfo.SchemaOutput.HasFlag(SchemaOutputOptions.Console);
            var outputFile = !schemaInfo.SchemaOutput.HasFlag(SchemaOutputOptions.None) &&
                              schemaInfo.SchemaOutput.HasFlag(SchemaOutputOptions.File) &&
                             !string.IsNullOrWhiteSpace(schemaInfo.SchemaOutputPath);

            using var writer = outputFile ? File.CreateText(schemaInfo.SchemaOutputPath!) : TextWriter.Null;
            new SchemaExport(configuration)
                .Execute(
                    useStdOut: outputConsole,
                    execute: true,
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
                .Register(ResolveConfiguration)
                .Named<Configuration>(CONFIGURATION_KEY)
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .Register(ctx => ResolveSessionFactory(ctx, SchemaInfo))
                .Named<ISessionFactory>(SESSION_FACTORY_KEY)
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .Register(ctx => ResolveSession(ctx, SchemaInfo))
                .As<ISession>()
                .SetLifetimeScope(LifetimeScopeType.PerScope);

            base.Load(builder);
        }

        #endregion
    }

    
}
