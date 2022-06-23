using System.Linq.Expressions;
using Nameless.IdentityServer.Entities;

namespace Nameless.IdentityServer.Repositories {

    public interface IUserRepository {

        #region Methods

        Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User> FindByIDAsync(Guid userID, CancellationToken cancellationToken = default);
        Task<User> FindByUserNameAsync(string username, CancellationToken cancellationToken = default);
        Task<User> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userID, CancellationToken cancellationToken = default);
        Task<IList<User>> ListAsync(Expression<Func<User, bool>>? filter = null, CancellationToken cancellationToken = default);

        Task AddClaimAsync(Guid userID, string claimType, string claimValue, CancellationToken cancellationToken = default);
        Task RemoveClaimAsync(UserClaim claim, CancellationToken cancellationToken = default);
        Task<IList<UserClaim>> ListClaimsAsync(Guid userID, CancellationToken cancellationToken = default);

        Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default);
        Task AddToRoleAsync(Guid userID, string roleName, CancellationToken cancellationToken = default);
        Task RemoveFromRoleAsync(Guid userID, string roleName, CancellationToken cancellationToken = default);
        Task<IList<string>> GetRolesAsync(Guid userID, CancellationToken cancellationToken = default);
        Task<bool> IsInRoleAsync(Guid userID, string roleName, CancellationToken cancellationToken = default);

        Task AddLoginAsync(UserLogin userLogin, CancellationToken cancellationToken = default);
        Task RemoveLoginAsync(UserLogin userLogin, CancellationToken cancellationToken = default);
        Task<IList<UserLogin>> ListLoginsAsync(Guid userID, CancellationToken cancellationToken = default);

        Task AddTokenAsync(UserToken userToken, CancellationToken cancellationToken = default);
        Task RemoveTokenAsync(UserToken userToken, CancellationToken cancellationToken = default);
        Task<IList<UserToken>> ListTokensAsync(Guid userID, CancellationToken cancellationToken = default);

        #endregion
    }
}
