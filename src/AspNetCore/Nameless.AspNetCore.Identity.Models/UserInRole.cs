using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class UserInRole : IdentityUserRole<Guid> {

        #region Public Virtual Methods

        public virtual bool Equals(UserInRole? obj) {
            return obj != null &&
                obj.UserId == UserId &&
                obj.RoleId == RoleId;
        }

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserInRole);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserId.GetHashCode() * 7;
                hash += RoleId.GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}
