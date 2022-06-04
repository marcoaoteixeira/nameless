using FluentMigrator.Runner;
using Nameless.NHibernate;

namespace Nameless.WebApplication.Web {
    
    public partial class StartUp {

        #region Public Methods

        public void UseMigration(IApplicationBuilder app) {
            using var scope = app.ApplicationServices.CreateScope();
            var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
            if (runner != null) { runner.MigrateUp(); }
        }

        public void ConfigureMigration(IServiceCollection services) {
            var opts = GetConfigurationFor<NHibernateOptions>(Configuration);

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(builder => {
                    builder
                        .AddSQLite()
                        .WithGlobalConnectionString(opts.Connection.ConnectionString)
                        .WithVersionTable<VersionTableAccessor>()
                        .ScanIn(typeof(StartUp).Assembly)
                        .For.Migrations();
                });
        }

        #endregion
    }
}
