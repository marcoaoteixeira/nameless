using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Nameless.Helpers;
using NHibernate;

namespace Nameless.AspNetCore.Identity.NHibernate {

    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role</typeparam>
    public class RoleStore<TRole> : RoleStore<TRole, Guid>
        where TRole : IdentityRole<Guid> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="RoleStore{TRole}"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(ISession session, IdentityErrorDescriber? describer = null) : base(session, describer) { }

        #endregion
    }

    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    public class RoleStore<TRole, TKey> : RoleStore<TRole, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>,
        IQueryableRoleStore<TRole>,
        IRoleClaimStore<TRole>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="RoleStore{TRole, TIdentityContext, TKey}"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(ISession session, IdentityErrorDescriber? describer = null) : base(session, describer) { }

        #endregion
    }

    /// <summary>
    /// Creates a new instance of a persistence store for roles.
    /// </summary>
    /// <typeparam name="TRole">The type of the class representing a role.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    /// <typeparam name="TUserRole">The type of the class representing a user role.</typeparam>
    /// <typeparam name="TRoleClaim">The type of the class representing a role claim.</typeparam>
    public class RoleStore<TRole, TKey, TUserRole, TRoleClaim> : RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new() {

        #region Public Override Properties

        public override IQueryable<TRole> Roles => Session.Query<TRole>();

        #endregion

        #region Protected Properties

        protected ISession Session { get; }

        #endregion

        #region Public Constructors

        public RoleStore(ISession session, IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber()) {
            Prevent.Null(session, nameof(session));

            Session = session;
        }

        #endregion

        #region Public Override Methods

        public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            await Session.SaveAsync(role, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();
            
            var currentRole = await Session.GetAsync<TRole>(role.Id, cancellationToken);

            if (currentRole == null) {
                throw new EntityNotFoundException(role.Id);
            }

            await Session.LockAsync(currentRole, LockMode.Upgrade, cancellationToken);
            await Session.MergeAsync(role, cancellationToken);

            currentRole.ConcurrencyStamp = int.TryParse(currentRole.ConcurrencyStamp, out var concurrencyStamp)
                ? (concurrencyStamp + 1).ToString()
                : "0";

            await transaction.CommitAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));

            cancellationToken.ThrowIfCancellationRequested();

            using var transaction = Session.BeginTransaction();

            var currentRole = await Session.GetAsync<TRole>(role.Id, cancellationToken);

            if (currentRole == null) {
                throw new EntityNotFoundException(role.Id);
            }

            await Session.LockAsync(currentRole, LockMode.Upgrade, cancellationToken);
            await Session.DeleteAsync(currentRole, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return IdentityResult.Success;
        }

        public override Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
            Prevent.NullEmptyOrWhiteSpace(id, nameof(id));

            if (!IDHelper.TryGetAs<TKey>(id, out var currentId)) {
                throw new InvalidIDCastException(nameof(id), id, typeof(TKey));
            }

            return Session
                .GetAsync<TRole>(Guid.Parse(id), cancellationToken);
        }

        public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default) {
            Prevent.NullEmptyOrWhiteSpace(normalizedName, nameof(normalizedName));

            var role = Session
                .Query<TRole>().SingleOrDefault(_ => _.NormalizedName == normalizedName);

            return Task.FromResult(role);
        }

        public override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));

            return Session
                .FindAsync<TRoleClaim>(
                    filter: _ => _.RoleId.Equals(role.Id),
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.FromRoleClaimList<TKey, TRoleClaim>());
        }

        public override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));
            Prevent.Null(claim, nameof(claim));

            var instruction = SaveInstruction<TRoleClaim>
                .Insert(new TRoleClaim {
                    RoleId = role.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });

            return Session.SaveAsync(instruction, cancellationToken);
        }

        public override Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) {
            Prevent.Null(role, nameof(role));
            Prevent.Null(claim, nameof(claim));

            var instruction = DeleteInstruction<TRoleClaim>
                .Create(filter: _ =>
                    _.RoleId.Equals(role.Id) &&
                    _.ClaimType == claim.Type &&
                    _.ClaimValue == claim.Value
                );

            return Session.DeleteAsync(instruction, cancellationToken);
        }

        #endregion
    }
}
