using System.Data.SQLite;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Nameless.IdentityServer.Entities;
using Nameless.IdentityServer.Infrastructure;
using Nameless.IdentityServer.Mappings;
using Nameless.NHibernate;
using NHibernate;

namespace Nameless.IdentityServer.UnitTesting {

    public abstract class DatabaseAwareTestBase {

        private ISessionFactory _sessionFactory;

        [OneTimeSetUp]
        public void OneTimeSetup() {
            var opts = new NHibernateOptions {
                Connection = new() {
                    ReleaseMode = ReleaseMode.OnClose
                },
                EntityRootTypes = new[] {
                    typeof(EntityBase).AssemblyQualifiedName!
                },
                MappingTypes = new[] {
                    typeof(RefreshTokenClassMapping).AssemblyQualifiedName!,
                    typeof(RoleClaimClassMapping).AssemblyQualifiedName!,
                    typeof(RoleClassMapping).AssemblyQualifiedName!,
                    typeof(UserClaimClassMapping).AssemblyQualifiedName!,
                    typeof(UserClassMapping).AssemblyQualifiedName!,
                    typeof(UserInRoleClassMapping).AssemblyQualifiedName!,
                    typeof(UserLoginClassMapping).AssemblyQualifiedName!,
                    typeof(UserTokenClassMapping).AssemblyQualifiedName!
                }
            };

            var configurationBuilder = new ConfigurationBuilder();
            var configuration = configurationBuilder.Build(opts);
            _sessionFactory = configuration.BuildSessionFactory();
        }

        protected ISession CreateSession(Guid id = default, bool runMigration = true) {
            var currentConnStr = $"Data Source=.\\{(id == Guid.Empty ? Guid.NewGuid() : id):N}.s3db;Version=3;Page Size=4096;";

            if (runMigration) {
                var serviceProvider = new ServiceCollection()
                    .AddFluentMigratorCore()
                    .ConfigureRunner(builder => {
                        builder
                            .AddSQLite()
                            .WithGlobalConnectionString(currentConnStr)
                            .WithVersionTable<VersionTableAccessor>()
                            .ScanIn(typeof(VersionTableAccessor).Assembly)
                            .For.Migrations();
                    })
                    .BuildServiceProvider(false);

                    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
                    runner.MigrateUp();
            }

            var session = _sessionFactory
                .WithOptions()
                .Connection(new SQLiteConnection(currentConnStr))
                .OpenSession();

            session.Connection.Open();

            return session;
        }
    }
}
