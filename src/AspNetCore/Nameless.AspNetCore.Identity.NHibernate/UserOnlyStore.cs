using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Nameless.Helpers;
using Nameless.Persistence;

namespace Nameless.AspNetCore.Identity.NHibernate {

    /// <summary>
    /// Creates a new instance of a persistence store for the specified user type.
    /// </summary>
    public class UserOnlyStore : UserOnlyStore<IdentityUser<Guid>> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserOnlyStore{TUser}"/>.
        /// </summary>
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public UserOnlyStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Creates a new instance of a persistence store for the specified user type.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    public class UserOnlyStore<TUser> : UserOnlyStore<TUser, Guid> where TUser : IdentityUser<Guid>, new() {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserOnlyStore{TUser}"/>.
        /// </summary>
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        public UserOnlyStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    public class UserOnlyStore<TUser, TKey> : UserOnlyStore<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey> {

        #region Public Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="UserStore{TUser, TRole, TIdentityContext, TKey}"/>.
        /// </summary>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/>.</param>
        /// <param name="repository">The <see cref="IRepository"/>.</param>
        public UserOnlyStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(repository, describer) { }

        #endregion
    }

    /// <summary>
    /// Represents a new instance of a persistence store for the specified user and role types.
    /// </summary>
    /// <typeparam name="TUser">The type representing a user.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for a role.</typeparam>
    /// <typeparam name="TUserClaim">The type representing a claim.</typeparam>
    /// <typeparam name="TUserLogin">The type representing a user external login.</typeparam>
    /// <typeparam name="TUserToken">The type representing a user token.</typeparam>
    public class UserOnlyStore<TUser, TKey, TUserClaim, TUserLogin, TUserToken> :
        UserStoreBase<TUser, TKey, TUserClaim, TUserLogin, TUserToken>,
        IProtectedUserStore<TUser>
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new() {

        #region Protected Properties

        public override IQueryable<TUser> Users => Repository.Query<TUser>();

        #endregion

        #region Protected Properties

        protected IRepository Repository { get; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a new instance of the store.
        /// </summary>
        /// <param name="repository">The <see cref="IRepository"/> used to access the underline storage.</param>
        /// <param name="describer">The <see cref="IdentityErrorDescriber"/> used to describe store errors.</param>
        public UserOnlyStore(IRepository repository, IdentityErrorDescriber? describer = null) : base(describer ?? new IdentityErrorDescriber()) {
            Prevent.Null(repository, nameof(repository));

            Repository = repository;
        }

        #endregion

        #region Public Override Methods

        public override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            var instruction = SaveInstruction<TUser>
                .Insert(user);

            return Repository
                .SaveAsync(instruction, cancellationToken)
                .ContinueWith(Internals.IdentityResultContinuation);
        }

        public override Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            var instruction = SaveInstruction<TUser>
                .Update(user, filter: _ => _.Id.Equals(user.Id));

            return Repository
                .SaveAsync(instruction, cancellationToken)
                .ContinueWith(Internals.IdentityResultContinuation);
        }

        public override Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            var instruction = DeleteInstruction<TUser>
                .Create(filter: _ => _.Id.Equals(user.Id));

            return Repository
                .DeleteAsync(instruction, cancellationToken)
                .ContinueWith(Internals.IdentityResultContinuation);
        }

        public override Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default) {
            if (!IDHelper.TryGetAs<TKey>(userId, out var currentId)) {
                throw new InvalidIDCastException(nameof(userId), userId, typeof(TKey));
            }

            return Repository
                .FindAsync<TUser>(
                    filter: _ => _.Id.Equals(currentId),
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.Single());
        }

        public override Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default) {
            return Repository
                .FindAsync<TUser>(
                    filter: _ => _.NormalizedUserName == normalizedUserName,
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.Single());
        }

        public override Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            return Repository
                .FindAsync<TUserClaim>(
                    filter: _ => _.UserId.Equals(user.Id),
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.FromUserClaimList<TKey, TUserClaim>());
        }

        public override Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            if (!claims.Any()) { return Task.CompletedTask; }

            var instructions = new SaveInstructionCollection<TUserClaim>();
            foreach (var claim in claims) {
                cancellationToken.ThrowIfCancellationRequested();
                instructions.Add(SaveInstruction<TUserClaim>
                    .Insert(
                        entity: new TUserClaim {
                            UserId = user.Id,
                            ClaimType = claim.Type,
                            ClaimValue = claim.Value
                        }
                    ));
            }

            return Repository.SaveAsync(instructions, cancellationToken: cancellationToken);
        }

        public override Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));
            Prevent.Null(claim, nameof(claim));
            Prevent.Null(newClaim, nameof(newClaim));

            var instruction = SaveInstruction<TUserClaim>
                .Update(
                    patch: _ => new TUserClaim {
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
            Prevent.Null(user, nameof(user));
            Prevent.Null(claims, nameof(claims));

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
            Prevent.Null(user, nameof(user));
            Prevent.Null(login, nameof(login));

            var instruction = SaveInstruction<TUserLogin>
                .Insert(
                    entity: new TUserLogin {
                        UserId = user.Id,
                        LoginProvider = login.LoginProvider,
                        ProviderKey = login.ProviderKey,
                        ProviderDisplayName = login.ProviderDisplayName
                    }
                );

            return Repository.SaveAsync(instruction, cancellationToken);
        }

        public override Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            var instruction = DeleteInstruction<TUserLogin>
                .Create(
                    filter: _ => _.UserId.Equals(user.Id) &&
                                 _.LoginProvider == loginProvider &&
                                 _.ProviderKey == providerKey
                );

            return Repository.DeleteAsync(instruction, cancellationToken);
        }

        public override Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default) {
            Prevent.Null(user, nameof(user));

            return Repository
                .FindAsync<TUserLogin>(
                    filter: _ => _.UserId.Equals(user.Id),
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.FromUserLoginList<TKey, TUserLogin>());
        }

        public override Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) {
            return Repository
                .FindAsync<TUser>(
                    filter: _ => _.NormalizedEmail == normalizedEmail,
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.Single());
        }

        public override Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default) {
            Prevent.Null(claim, nameof(claim));

            var users = Repository.Query<TUser>();
            var usersClaims = Repository.Query<TUserClaim>();

            var query = from userClaim in usersClaims
                        where userClaim.ClaimType == claim.Type && userClaim.ClaimValue == claim.Value
                        join user in users.AsQueryable() on userClaim.UserId equals user.Id
                        select user;

            return Task.FromResult((IList<TUser>)query.ToList());
        }

        #endregion

        #region Protected Override Methods

        protected override Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken) {
            return Repository
                .FindAsync<TUser>(
                    filter: _ => _.Id.Equals(userId),
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.Single());
        }

        protected override Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken) {
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
                .FindAsync(filter, cancellationToken: cancellationToken)
                .ContinueWith(antecedent => antecedent.Result.Single());
        }

        protected override Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) {
            return FindUserLoginAsync(default!, loginProvider, providerKey, cancellationToken);
        }

        protected override Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken) {
            Prevent.Null(user, nameof(user));

            return Repository
                .FindAsync<TUserToken>(
                    filter: _ => _.UserId.Equals(user.Id) &&
                                 _.LoginProvider == loginProvider &&
                                 _.Name == name,
                    cancellationToken: cancellationToken
                )
                .ContinueWith(antecedent => antecedent.Result.Single());
        }

        protected override Task AddUserTokenAsync(TUserToken token) {
            Prevent.Null(token, nameof(token));

            var instruction = SaveInstruction<TUserToken>
                .Insert(token);

            return Repository.SaveAsync(instruction, cancellationToken: default);
        }

        protected override Task RemoveUserTokenAsync(TUserToken token) {
            Prevent.Null(token, nameof(token));

            var instruction = DeleteInstruction<TUserToken>
                .Create(
                    filter: _ => _.UserId.Equals(token.UserId) &&
                                 _.LoginProvider == token.LoginProvider &&
                                 _.Name == token.Name
                );

            return Repository.DeleteAsync(instruction);
        }

        #endregion
    }
}
