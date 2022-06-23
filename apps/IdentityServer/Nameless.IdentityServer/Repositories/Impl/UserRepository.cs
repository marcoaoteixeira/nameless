using System.Linq.Expressions;
using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Linq;

namespace Nameless.IdentityServer.Repositories {

    public sealed class UserRepository : IUserRepository {

        #region Private Properties

        private ISession Session { get; }

        #endregion

        #region Public Constructors

        public UserRepository(ISession session) {
            Prevent.Null(session, nameof(session));

            Session = session;
        }

        #endregion

        #region IUserRepository Members

        public async Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            var result = await Session.SaveAsync(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (Guid)result;
        }

        public Task<User> FindByIDAsync(Guid userID, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            return Session.GetAsync<User>(userID, cancellationToken);
        }

        public Task<User> FindByUserNameAsync(string username, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            return Session.Query<User>().SingleOrDefaultAsync(_ => _.Username == username, cancellationToken);
        }

        public Task<User> FindByEmailAsync(string email, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            return Session.Query<User>().SingleOrDefaultAsync(_ => _.Email == email, cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            var current = Session.GetAsync<Role>(user.ID, LockMode.Force, cancellationToken);
            if (current == null) { throw new Exception("Entity not found."); }
            await Session.MergeAsync(user, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid userID, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            var user = await Session.GetAsync<User>(userID, LockMode.Upgrade, cancellationToken);
            if (user != null) {
                using var transaction = Session.BeginTransaction();
                await Session.DeleteAsync(user, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
        }

        public Task<IList<User>> ListAsync(Expression<Func<User, bool>>? filter = null, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

        public Task AddClaimAsync(Guid userID, string claimType, string claimValue, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task RemoveClaimAsync(UserClaim claim, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task<IList<UserClaim>> ListClaimsAsync(Guid userID, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

        public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task AddToRoleAsync(Guid userID, string roleName, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task RemoveFromRoleAsync(Guid userID, string roleName, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task<IList<string>> GetRolesAsync(Guid userID, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task<bool> IsInRoleAsync(Guid userID, string roleName, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

        public Task AddLoginAsync(UserLogin userLogin, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task RemoveLoginAsync(UserLogin userLogin, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task<IList<UserLogin>> ListLoginsAsync(Guid userID, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }

        public Task AddTokenAsync(UserToken userToken, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task RemoveTokenAsync(UserToken userToken, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }
        public Task<IList<UserToken>> ListTokensAsync(Guid userID, CancellationToken cancellationToken = default) { throw new NotImplementedException(); }


        #endregion
    }
}
