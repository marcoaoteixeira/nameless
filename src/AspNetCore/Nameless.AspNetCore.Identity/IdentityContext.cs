using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity {

    public class IdentityContext : IdentityContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid> { }

    public class IdentityContext<TUser> : IdentityContext<TUser, IdentityRole<Guid>, Guid>
        where TUser : IdentityUser<Guid> { }

    public class IdentityContext<TUser, TRole, TKey> : IdentityContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey> { }

    public abstract class IdentityContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> : IIdentityContext
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserToken : IdentityUserToken<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey> { }
}
