using Nameless.IdentityServer.Entities;
using Nameless.IdentityServer.Repositories;

namespace Nameless.IdentityServer.UnitTesting {

    public class RoleRepositoryTest : DatabaseAwareTestBase {

        [Test]
        public async Task Create_New_Role() {
            var sessionID = Guid.NewGuid();
            Guid id;

            using (var session = CreateSession(sessionID, runMigration: true)) {
                RoleRepository roleRepository = new(session);
                id = await roleRepository.CreateAsync(new Role {
                    Name = "ADMIN"
                });
            }

            using (var session = CreateSession(sessionID, runMigration: false)) {
                RoleRepository roleRepository = new(session);
                await roleRepository.UpdateAsync(new(id) {
                    Name = "Sys_Admin"
                });
            }

            Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        }
    }
}
