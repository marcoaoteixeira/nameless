using System.Data;
using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Data.MSSQLServer {

    /// <summary>
    /// The data common service registration.
    /// </summary>
    public sealed class DataModule : ModuleBase {

        #region Private Constants

        private const string DB_CONNECTION_PROVIDER_KEY = "7e99f8b1-05ad-4a89-8e36-46a7660bc8a8";

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder
                .RegisterType<DbConnectionProvider>()
                .Named<IDbConnectionProvider>(DB_CONNECTION_PROVIDER_KEY)
                .WithParameter(
                    parameterSelector: (param, ctx) => param.ParameterType == typeof(DatabaseOptions),
                    valueProvider: (param, ctx) => ctx.ResolveOptional<DatabaseOptions>() ?? new()
                )
                .SingleInstance();

            builder
                .RegisterType<Database>()
                .As<IDatabase>()
                .WithParameter(
                    parameterSelector: (param, ctx) => param.ParameterType == typeof(IDbConnection),
                    valueProvider: (param, ctx) => ctx.ResolveNamed<IDbConnectionProvider>(DB_CONNECTION_PROVIDER_KEY).GetConnection()
                )
                .InstancePerLifetimeScope();

            base.Load(builder);
        }

        #endregion
    }
}
