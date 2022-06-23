using System.Linq.Expressions;
using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Linq;

namespace Nameless.IdentityServer.Repositories {

    public sealed class RoleRepository : IRoleRepository {

        #region Private Properties

        private ISession Session { get; }

        #endregion

        #region Public Constructors

        public RoleRepository(ISession session) {
            Prevent.Null(session, nameof(session));

            Session = session;
        }

        #endregion

        #region IRoleRepository Members

        public async Task<Guid> CreateAsync(Role role, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            var result = await Session.SaveAsync(role, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return (Guid)result;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            var role = await Session.GetAsync<Role>(id, LockMode.Upgrade, cancellationToken);
            if (role != null) {
                using var transaction = Session.BeginTransaction();
                await Session.DeleteAsync(role, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
        }

        public Task<Role> FindByIDAsync(Guid id, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            return Session.GetAsync<Role>(id, cancellationToken);
        }

        public Task<Role> FindByNameAsync(string name, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            return Session.Query<Role>().SingleOrDefaultAsync(_ => _.Name == name, cancellationToken);
        }

        public Task<IList<Role>> ListAsync(Expression<Func<Role, bool>>? filter = null, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            var query = Session.Query<Role>();
            if (filter != null) {
                query = query.Where(filter);
            }
            var result = query.ToList() as IList<Role>;

            return Task.FromResult(result);
        }

        public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            var current = Session.GetAsync<Role>(role.ID, LockMode.Force, cancellationToken);
            if (current == null) { throw new Exception("Entity not found."); }
            await Session.MergeAsync(role, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task AddClaimAsync(Guid roleID, string claimType, string claimValue, CancellationToken cancellationToken = default) {
            Prevent.Default(roleID, nameof(roleID));
            Prevent.NullEmptyOrWhiteSpace(claimType, nameof(claimType));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            await Session.SaveAsync(new RoleClaim {
                RoleID = roleID,
                Type = claimType,
                Value = claimValue
            }, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        public async Task RemoveClaimAsync(RoleClaim claim, CancellationToken cancellationToken = default) {
            Prevent.Null(claim, nameof(claim));

            cancellationToken.ThrowIfCancellationRequested();

            var role = await Session
                .Query<RoleClaim>()
                .WithLock(LockMode.Upgrade)
                .SingleOrDefaultAsync(_ => _.RoleID == claim.RoleID && _.Type == claim.Type, cancellationToken: cancellationToken);
            if (role != null) {
                using var transaction = Session.BeginTransaction();
                await Session.DeleteAsync(role, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
        }

        public async Task<IList<RoleClaim>> ListClaimsAsync(Guid roleID, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();

            return await Session
                .Query<RoleClaim>()
                .Where(_ => _.RoleID == roleID)
                .ToListAsync(cancellationToken);
        }

        #endregion
    }
}
