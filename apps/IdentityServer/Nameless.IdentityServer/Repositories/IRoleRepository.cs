using System.Linq.Expressions;
using Nameless.IdentityServer.Entities;

namespace Nameless.IdentityServer.Repositories {

    public interface IRoleRepository {

        #region Members

        Task<Guid> CreateAsync(Role role, CancellationToken cancellationToken = default);
        Task<Role> FindByIDAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Role> FindByNameAsync(string name, CancellationToken cancellationToken = default);
        Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IList<Role>> ListAsync(Expression<Func<Role, bool>>? filter = null, CancellationToken cancellationToken = default);
        Task AddClaimAsync(Guid roleID, string claimType, string claimValue, CancellationToken cancellationToken = default);
        Task RemoveClaimAsync(RoleClaim claim, CancellationToken cancellationToken = default);
        Task<IList<RoleClaim>> ListClaimsAsync(Guid roleID, CancellationToken cancellationToken = default);

        #endregion
    }
}
