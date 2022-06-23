using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class UserToken : IdentityUserToken<Guid> {

        #region Public Virtual Methods

        public virtual bool Equals(UserToken? obj) {
            return obj != null &&
                obj.UserId == UserId &&
                obj.LoginProvider == LoginProvider &&
                obj.Name == Name;
        }

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserToken);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserId.GetHashCode() * 7;
                hash += (LoginProvider ?? string.Empty).GetHashCode() * 7;
                hash += (Name ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}