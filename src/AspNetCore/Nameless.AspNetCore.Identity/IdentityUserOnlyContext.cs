using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity {

    public class IdentityUserOnlyContext<TUser> : IdentityUserOnlyContext<TUser, Guid>
        where TUser : IdentityUser<Guid> { }

    public class IdentityUserOnlyContext<TUser, TKey> : IdentityUserOnlyContext<TUser, TKey, IdentityUserClaim<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>>
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey> { }

    public abstract class IdentityUserOnlyContext<TUser, TKey, TUserClaim, TUserLogin, TUserToken> : IIdentityContext
        where TUser : IdentityUser<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TUserToken : IdentityUserToken<TKey> { }
}
