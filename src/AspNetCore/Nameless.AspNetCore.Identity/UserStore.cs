using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Nameless.Persistence;

namespace Nameless.AspNetCore.Identity {

    /// <summary>
    /// Represents a new instance of a persistence store for users, using the default implementation
    /// of <see cref="IdentityUser{TKey}"/> with a string as a primary key.
    /// </summary>
    public class UserStore : UserStore<IdentityUser<Guid>> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserStore"/>.
        /// </summary>
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public UserStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Creates a new instance of a persistence store for the specified user type.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    public class UserStore<TUser> : UserStore<TUser, IdentityRole<Guid>, Guid>
        where TUser : IdentityUser<Guid>, new() {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserStore{TUser}"/>.
        /// </summary>
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public UserStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TRole">The type representing a role.</typeparam>
    public class UserStore<TUser, TRole> : UserStore<TUser, TRole, Guid>
        where TUser : IdentityUser<Guid>
        where TRole : IdentityRole<Guid> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TIdentityContext}"/>.
        /// </summary>
        /// <param name="repository">The <see cref="DbContext"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public UserStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TRole">The type representing a role.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    public class UserStore<TUser, TRole, TKey> : UserStore<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TIdentityContext, TKey}"/>.
        /// </summary>
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public UserStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TRole">The type representing a role.</typeparam>
    /// <typeparam name="TIdentityContext">The type of the data repository class used to access the store.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    /// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
    /// <typeparam name="TUserRole">The type representing a user role.</typeparam>
    /// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
    /// <typeparam name="TUserToken">The type representing a user token.</typeparam>
    /// <typeparam name="TRoleClaim">The type representing a role claim.</typeparam>
    public class UserStore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> : UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>, IProtectedUserStore<TUser>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new() {

        #region Public Properties

        public IRepository Repository { get; set; }

        #endregion

        #region Public Override Properties

        public override IQueryable<TUser> Users => Repository.Query<TUser>();

        #endregion

        #region Public Constructors

        public UserStore(IRepository repository, IdentityErrorDescriber? describer) : base(describer) {
            Ensure.NotNull(repository, nameof(repository));

            Repository = repository;
        }

        #endregion

        #region Public Override Methods

        public override Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default) {
            Ensure.NotNullEmptyOrWhiteSpace(normalizedRoleName, nameof(normalizedRoleName));

            var users = Repository.Query<TUser>();
            var usersRoles = Repository.Query<TUserRole>();
            var roles = Repository.Query<TRole>();

            var query = from userRole in usersRoles
                        join user in users on userRole.UserId equals user.Id
                        join role in roles on userRole.RoleId equals role.Id
                        where role.NormalizedName == normalizedRoleName
                        select user;

            return Task.FromResult((IList<TUser>)query.ToList());
        }

        public override Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNullEmptyOrWhiteSpace(normalizedRoleName, nameof(normalizedRoleName));

            return FindRoleAsync(normalizedRoleName, cancellationToken)
                .ContinueWith(antecedent => {
                    var role = antecedent.Result;
                    if (role == null) { return; }

                    var instruction = new SaveInstruction<TUserRole>(
                        entity: new TUserRole {
                            UserId = user.Id,
                            RoleId = role.Id
                        },
                        filter: _ => _.UserId.Equals(user.Id) &&
                                     _.RoleId.Equals(role.Id)
                    );

                    Repository.SaveAsync(instruction, cancellationToken);
                }, cancellationToken);
        }

        public override Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNullEmptyOrWhiteSpace(normalizedRoleName, nameof(normalizedRoleName));

            return FindRoleAsync(normalizedRoleName, cancellationToken)
                .ContinueWith(antecedent => {
                    var role = antecedent.Result;
                    if (role == null) { return; }

                    var instruction = new DeleteInstruction<TUserRole>(
                        filter: _ => _.UserId.Equals(user.Id) &&
                                     _.RoleId.Equals(role.Id)
                    );

                    Repository.DeleteAsync(instruction, cancellationToken);
                }, cancellationToken);
        }

        public override Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));

            var userRoleCollection = Repository.Query<TUserRole>();
            var roleCollection = Repository.Query<TRole>();

            var query = from userRole in userRoleCollection
                        join role in roleCollection on new { userRole.UserId, userRole.RoleId } equals
                                                       new { UserId = user.Id, RoleId = role.Id }
                        select role.Name;

            return Task.FromResult((IList<string>)query.ToList());
        }

        public override Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));

            var userRoleCollection = Repository.Query<TUserRole>();
            var roleCollection = Repository.Query<TRole>();

            var query = from userRole in userRoleCollection
                        join role in roleCollection on new { userRole.UserId, userRole.RoleId } equals
                                                       new { UserId = user.Id, RoleId = role.Id }
                        where role.NormalizedName == normalizedRoleName
                        select role;

            return Task.FromResult(query.Any());
        }

        public override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default) {
            return SaveAsync(user, cancellationToken);
        }

        public override Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default) {
            return SaveAsync(user, cancellationToken);
        }

        public override Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));

            var instruction = new DeleteInstruction<TUser>(
                filter: _ => _.Id.Equals(user.Id)
            );

            return Repository
                .DeleteAsync(instruction, cancellationToken)
                .ContinueWith(Utils.IdentityResultContinuation);
        }

        public override Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default) {
            Ensure.NotNullEmptyOrWhiteSpace(userId, nameof(userId));

            var curretId = Utils.Parse<TKey>(userId);

            return Repository
                .FindAsync<TUser>(_ => _.Id.Equals(curretId), cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        public override Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default) {
            Ensure.NotNullEmptyOrWhiteSpace(normalizedUserName, nameof(normalizedUserName));

            return Repository
                .FindAsync<TUser>(_ => _.NormalizedUserName == normalizedUserName, cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));

            return Repository
                .FindAsync<TUserClaim>(_ => _.UserId.Equals(user.Id), cancellationToken)
                .Project(_ => _.ToClaim(), cancellationToken)
                .ToListAsync(cancellationToken);
        }

        public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));

            if (!claims.Any()) { return Task.CompletedTask; }

            var instructions = new SaveInstructionCollection<TUserClaim>();
            foreach (var claim in claims) {
                cancellationToken.ThrowIfCancellationRequested();

                instructions.Add(
                    entity: new TUserClaim {
                        UserId = user.Id,
                        ClaimType = claim.Type,
                        ClaimValue = claim.Value
                    },
                    filter: _ => _.UserId.Equals(user.Id) &&
                                 _.ClaimType == claim.Type &&
                                 _.ClaimValue == claim.Value
                );
            }

            return Repository.SaveAsync(instructions, cancellationToken: cancellationToken);
        }

        public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNull(claim, nameof(claim));
            Ensure.NotNull(newClaim, nameof(newClaim));

            var instruction = new SaveInstruction<TUserClaim>(
                entity: new TUserClaim {
                    UserId = user.Id,
                    ClaimType = newClaim.Type,
                    ClaimValue = newClaim.Value
                },
                filter: _ => _.UserId.Equals(user.Id) &&
                             _.ClaimType == claim.Type &&
                             _.ClaimValue == claim.Value
            );

            return Repository.SaveAsync(instruction, cancellationToken);
        }

        public override Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNull(claims, nameof(claims));

            if (!claims.Any()) { return Task.CompletedTask; }

            var instructions = new DeleteInstructionCollection<TUserClaim>();
            foreach (var claim in claims) {
                cancellationToken.ThrowIfCancellationRequested();

                instructions.Add(_ =>
                    _.UserId.Equals(user.Id) &&
                    _.ClaimType == claim.Type &&
                    _.ClaimValue == claim.Value
                );
            }

            return Repository.DeleteAsync(instructions, cancellationToken: cancellationToken);
        }

        public override Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNull(login, nameof(login));

            var instruction = new SaveInstruction<TUserLogin>(
                entity: new TUserLogin {
                    UserId = user.Id,
                    LoginProvider = login.LoginProvider,
                    ProviderKey = login.ProviderKey,
                    ProviderDisplayName = login.ProviderDisplayName
                },
                filter: _ => _.UserId.Equals(user.Id) &&
                             _.LoginProvider == login.LoginProvider &&
                             _.ProviderKey == login.ProviderKey
            );

            return Repository.SaveAsync(instruction, cancellationToken);
        }

        public override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNullEmptyOrWhiteSpace(loginProvider, nameof(loginProvider));
            Ensure.NotNullEmptyOrWhiteSpace(providerKey, nameof(providerKey));

            var instruction = new DeleteInstruction<TUserLogin>(
                filter: _ => _.UserId.Equals(user.Id) &&
                             _.LoginProvider == loginProvider &&
                             _.ProviderKey == providerKey
            );

            return Repository.DeleteAsync(instruction, cancellationToken);
        }

        public override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));

            return Repository
                .FindAsync<TUserLogin>(_ => _.UserId.Equals(user.Id), cancellationToken)
                .Project(_ => new UserLoginInfo(
                            _.LoginProvider,
                            _.ProviderKey,
                            _.ProviderDisplayName
                        ), cancellationToken)
                .ToListAsync(cancellationToken);
        }

        public override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) {
            Ensure.NotNullEmptyOrWhiteSpace(normalizedEmail, nameof(normalizedEmail));

            return Repository
                .FindAsync<TUser>(_ => _.NormalizedEmail == normalizedEmail, cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        public override Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) {
            Ensure.NotNull(claim, nameof(claim));

            var users = Repository.Query<TUser>();
            var usersClaims = Repository.Query<TUserClaim>();

            var query = from userClaim in usersClaims.AsQueryable()
                        where userClaim.ClaimType == claim.Type && userClaim.ClaimValue == claim.Value
                        join user in users.AsQueryable() on userClaim.UserId equals user.Id
                        select user;

            return Task.FromResult((IList<TUser>)query.ToList());
        }

        #endregion

        #region Protected Override Methods

        protected override Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken) {
            Ensure.NotNullEmptyOrWhiteSpace(normalizedRoleName, nameof(normalizedRoleName));

            return Repository
                .FindAsync<TRole>(
                    filter: _ => _.NormalizedName == normalizedRoleName,
                    cancellationToken: cancellationToken
                ).FirstOrDefault(cancellationToken);
        }

        protected override Task<TUserRole> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken) {
            return Repository
                .FindAsync<TUserRole>(
                    filter: _ => _.UserId.Equals(userId) &&
                                 _.RoleId.Equals(roleId),
                    cancellationToken: cancellationToken
                ).FirstOrDefault(cancellationToken);
        }

        protected override Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken) {
            return Repository
                .FindAsync<TUser>(_ => _.Id.Equals(userId), cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        protected override Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken) {
            Ensure.NotNullEmptyOrWhiteSpace(loginProvider, nameof(loginProvider));
            Ensure.NotNullEmptyOrWhiteSpace(providerKey, nameof(providerKey));

            Expression<Func<TUserLogin, bool>> filter;

            // userId has value?
            if (!EqualityComparer<TKey>.Default.Equals(userId, default)) {
                filter = _ =>
                    _.UserId.Equals(userId) &&
                    _.LoginProvider == loginProvider &&
                    _.ProviderKey == providerKey;
            } else {
                filter = _ =>
                    _.LoginProvider == loginProvider &&
                    _.ProviderKey == providerKey;
            }

            return Repository
                .FindAsync(filter, cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        protected override Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) {
            return FindUserLoginAsync(default!, loginProvider, providerKey, cancellationToken);
        }

        protected override Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNullEmptyOrWhiteSpace(loginProvider, nameof(loginProvider));
            Ensure.NotNullEmptyOrWhiteSpace(name, nameof(name));

            return Repository
                .FindAsync<TUserToken>(_ => _.UserId.Equals(user.Id) &&
                                            _.LoginProvider == loginProvider &&
                                            _.Name == name, cancellationToken)
                .FirstOrDefault(cancellationToken);
        }

        protected override Task AddUserTokenAsync(TUserToken token) {
            Ensure.NotNull(token, nameof(token));

            var instruction = new SaveInstruction<TUserToken>(
                entity: token,
                filter: _ => _.UserId.Equals(token.UserId) &&
                             _.LoginProvider == token.LoginProvider &&
                             _.Name == token.Name
            );

            return Repository.SaveAsync(instruction);
        }

        protected override Task RemoveUserTokenAsync(TUserToken token) {
            Ensure.NotNull(token, nameof(token));

            var instruction = new DeleteInstruction<TUserToken>(
                filter: _ => _.UserId.Equals(token.UserId) &&
                             _.LoginProvider == token.LoginProvider &&
                             _.Name == token.Name
            );

            return Repository.DeleteAsync(instruction);
        }

        #endregion

        #region Private Methods

        private Task<IdentityResult> SaveAsync(TUser user, CancellationToken cancellationToken) {
            var instruction = new SaveInstruction<TUser>(
                entity: user,
                filter: _ => _.Id.Equals(user.Id)
            );

            return Repository
                .SaveAsync(instruction, cancellationToken)
                .ContinueWith(Utils.IdentityResultContinuation);
        }

        #endregion
    }
}