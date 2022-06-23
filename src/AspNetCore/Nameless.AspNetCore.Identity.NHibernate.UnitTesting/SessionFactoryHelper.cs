using System.Data.SQLite;
using Microsoft.AspNetCore.Identity;
using Nameless.AspNetCore.Identity.NHibernate.Mappings;
using Nameless.NHibernate;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Nameless.AspNetCore.Identity.UnitTesting {

    internal sealed class SessionFactoryHelper : IDisposable {

        private readonly Configuration _configuration;

        private ISessionFactory? _sessionFactory;
        private bool _disposed;

        internal Configuration Configuration => _configuration;

        internal SessionFactoryHelper(NHibernateOptions? options = null) {
            var opts = options ?? new NHibernateOptions {
                Connection = new() {
                    ReleaseMode = ReleaseMode.OnClose
                },
                EntityRootTypes = new[] {
                    typeof(IdentityUser<>).AssemblyQualifiedName!,
                    typeof(IdentityUserClaim<>).AssemblyQualifiedName!,
                    typeof(IdentityUserLogin<>).AssemblyQualifiedName!,
                    typeof(IdentityUserToken<>).AssemblyQualifiedName!,
                    typeof(IdentityRole<>).AssemblyQualifiedName!,
                    typeof(IdentityRoleClaim<>).AssemblyQualifiedName!,
                    typeof(IdentityUserRole<>).AssemblyQualifiedName!,
                },
                MappingTypes = new[] {
                    typeof(UserClassMapping).AssemblyQualifiedName!,
                    typeof(UserClaimClassMapping).AssemblyQualifiedName!,
                    typeof(UserLoginClassMapping).AssemblyQualifiedName!,
                    typeof(UserTokenClassMapping).AssemblyQualifiedName!,
                    typeof(RoleClassMapping).AssemblyQualifiedName!,
                    typeof(RoleClaimClassMapping).AssemblyQualifiedName!,
                    typeof(UserInRoleClassMapping).AssemblyQualifiedName!,
                }
            };

            var configurationBuilder = new ConfigurationBuilder();
            _configuration = configurationBuilder.Build(opts);
            _sessionFactory = _configuration.BuildSessionFactory();
        }

        ~SessionFactoryHelper() {
            Dispose(false);
        }

        internal ISession OpenSession(Guid id = default, bool executeSchemaExport = false) {
            BlockAccessAfterDispose();

            var currentID = id == Guid.Empty ? Guid.NewGuid() : id;

            var session = _sessionFactory!
                .WithOptions()
                .Connection(new SQLiteConnection(GetConnStr(currentID)))
                .OpenSession();

            session.Connection.Open();

            if (executeSchemaExport) {
                ExecuteSchemaExport(session, Configuration);
            }

            return session;
        }

        private static void ExecuteSchemaExport(ISession session, Configuration configuration) {
            new SchemaExport(configuration)
                .Execute(
                    useStdOut: true,
                    execute: true,
                    justDrop: false,
                    connection: session.Connection,
                    exportOutput: TextWriter.Null
                );
        }

        private static string GetConnStr(Guid id) {
            var currentID = id == Guid.Empty ? Guid.NewGuid() : id;

            return $"Data Source=.\\{currentID:N}.s3db;Version=3;Page Size=4096;";
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(SessionFactoryHelper));
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _sessionFactory?.Close();
                _sessionFactory?.Dispose();
            }

            _sessionFactory = null;
            _disposed = true;
        }

        void IDisposable.Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
