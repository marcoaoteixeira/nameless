using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Nameless.Persistence;

namespace Nameless.AspNetCore.Identity {

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
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

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
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public RoleStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

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

        public override IQueryable<TRole> Roles => Repository.Query<TRole>();

        #endregion

        #region Protected Properties

        protected IRepository Repository { get; }

        #endregion

        #region Public Constructors

        public RoleStore(IRepository repository, IdentityErrorDescriber? describer) : base(describer) {
            Ensure.NotNull(repository, nameof(repository));

            Repository = repository;
        }

        #endregion

        #region Public Override Methods

        public override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default) {
            return SaveAsync(role, cancellationToken);
        }

        public override Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default) {
            return SaveAsync(role, cancellationToken);
        }

        public override Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default) {
            Ensure.NotNull(role, nameof(role));

            var instruction = new DeleteInstruction<TRole>(_ => _.Id.Equals(role.Id));
            return Repository
                .DeleteAsync(instruction, cancellationToken)
                .ContinueWith(Utils.IdentityResultContinuation);
        }

        public override Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default) {
            Ensure.NotNullEmptyOrWhiteSpace(id, nameof(id));

            var currentId = Utils.Parse<TKey>(id);

            return Repository.FindAsync<TRole>(_ => _.Id.Equals(currentId), cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default) {
            Ensure.NotNullEmptyOrWhiteSpace(normalizedName, nameof(normalizedName));

            return Repository
                .FindAsync<TRole>(_ => _.NormalizedName == normalizedName, cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        public override Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default) {
            Ensure.NotNull(role, nameof(role));

            return Repository
                .FindAsync<TRoleClaim>(_ => EqualityComparer<TKey>.Default.Equals(_.RoleId, role.Id), cancellationToken)
                .Project(_ => _.ToClaim(), cancellationToken)
                .ToListAsync(cancellationToken);
        }

        public override Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) {
            Ensure.NotNull(role, nameof(role));
            Ensure.NotNull(claim, nameof(claim));

            var instruction = new SaveInstruction<TRoleClaim>(
                entity: new TRoleClaim {
                    RoleId = role.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                },
                filter: _ => _.RoleId.Equals(role.Id) &&
                             _.ClaimType == claim.Type &&
                             _.ClaimValue == claim.Value
            );

            return Repository.SaveAsync(instruction, cancellationToken);
        }

        public override Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default) {
            Ensure.NotNull(role, nameof(role));
            Ensure.NotNull(claim, nameof(claim));

            var instruction = new DeleteInstruction<TRoleClaim>(
                filter: _ => _.RoleId.Equals(role.Id) &&
                             _.ClaimType == claim.Type &&
                             _.ClaimValue == claim.Value
            );

            return Repository.DeleteAsync(instruction, cancellationToken);
        }

        #endregion

        #region Private Methods

        private Task<IdentityResult> SaveAsync(TRole role, CancellationToken cancellationToken) {
            var instruction = new SaveInstruction<TRole>(
                entity: role,
                filter: _ => _.Id.Equals(role.Id)
            );

            return Repository
                .SaveAsync(instruction, cancellationToken)
                .ContinueWith(Utils.IdentityResultContinuation);
        }

        #endregion
    }
}
