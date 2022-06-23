using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Nameless.IdentityServer.Infrastructure;

namespace Nameless.IdentityServer.UnitTesting {

    public class MigrationTest {

        IServiceProvider _serviceProvider;

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            _serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(builder => {
                    builder
                        //.AddSqlServer()
                        //.WithGlobalConnectionString("Server=(localdb)\\MSSQLLocalDB;Database=Nameless;Integrated Security=true;")
                        .AddSQLite()
                        .WithGlobalConnectionString($"Data Source=.\\{Guid.NewGuid():N}.s3db;Version=3;Page Size=4096;")
                        .WithVersionTable<VersionTableAccessor>()
                        .ScanIn(typeof(VersionTableAccessor).Assembly)
                        .For.Migrations();
                })
                .BuildServiceProvider(false);
        }

        [Test]
        public void Run_Migration() {
            var runner = _serviceProvider.GetRequiredService<IMigrationRunner>();

            // Execute the migrations
            runner.MigrateUp();

            Assert.Pass();
        }
    }
}