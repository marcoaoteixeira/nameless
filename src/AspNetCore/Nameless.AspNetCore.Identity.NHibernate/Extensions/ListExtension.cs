using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.NHibernate {

    internal static class ListExtension {

        #region Internal Static Methods

        internal static IList<Claim> FromRoleClaimList<TKey, TRoleClaim>(this IList<TRoleClaim> self)
            where TKey : IEquatable<TKey>
            where TRoleClaim : IdentityRoleClaim<TKey>, new() {
            Prevent.Null(self, nameof(self));

            return self.Select(_ => _.ToClaim()).ToList();
        }

        internal static IList<Claim> FromUserClaimList<TKey, TUserClaim>(this IList<TUserClaim> self)
            where TKey : IEquatable<TKey>
            where TUserClaim : IdentityUserClaim<TKey>, new() {
            Prevent.Null(self, nameof(self));

            return self.Select(_ => _.ToClaim()).ToList();
        }

        internal static IList<UserLoginInfo> FromUserLoginList<TKey, TUserLogin>(this IList<TUserLogin> self)
            where TKey : IEquatable<TKey>
            where TUserLogin : IdentityUserLogin<TKey>, new() {
            Prevent.Null(self, nameof(self));

            return self.Select(_ =>
                new UserLoginInfo(
                    _.LoginProvider,
                    _.ProviderKey,
                    _.ProviderDisplayName
                )
            ).ToList();
        }

        #endregion
    }
}
