using Nameless.AspNetCore.Identity.Models;
using Nameless.Persistence;
using Nameless.Persistence.NHibernate;

namespace Nameless.AspNetCore.Identity.UnitTesting {

    public class RoleStoreTest {

        private SessionFactoryHelper _sessionFactoryHelper;

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            _sessionFactoryHelper = new SessionFactoryHelper();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() {
            ((IDisposable)_sessionFactoryHelper)?.Dispose();
        }

        [Test]
        public async Task CreateAsync_Should_Create_A_New_Role() {
            var expected = "NEW_ROLE";

            var session = _sessionFactoryHelper.OpenSession(executeSchemaExport: true);
            var writer = new Writer(session);
            var reader = new Reader(session);
            var repository = new Repository(writer, reader);
            var roleStore = new RoleStore<Role, Guid, UserInRole, RoleClaim>(repository, null);

            var result = await roleStore.CreateAsync(new Role {
                Name = expected,
            });

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Succeeded, Is.True);
        }
    }
}
