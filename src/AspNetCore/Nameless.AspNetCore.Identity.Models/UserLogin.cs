using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class UserLogin : IdentityUserLogin<Guid> {

        #region Public Virtual Methods

        public virtual bool Equals(UserLogin? obj) {
            return obj != null &&
                obj.UserId == UserId &&
                obj.LoginProvider == LoginProvider &&
                obj.ProviderKey == ProviderKey;
        }

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserLogin);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserId.GetHashCode() * 7;
                hash += (LoginProvider ?? string.Empty).GetHashCode() * 7;
                hash += (ProviderKey ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}