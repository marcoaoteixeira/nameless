using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class UserClaim : IdentityUserClaim<Guid> {

        #region Private Constants

        private const string ID_PROP = "Id";
        private const string USER_ID_PROP = "UserId";

        #endregion

        #region Public Virtual Methods

        public override void InitializeFromClaim(Claim other) {
            ClaimType = other.Type;
            ClaimValue = other.Value;

            if (other.Properties.TryGetValue(ID_PROP, out var id)) {
                Id = int.Parse(id);
            }
            if (other.Properties.TryGetValue(USER_ID_PROP, out var userId)) {
                UserId = Guid.Parse(userId);
            }
        }

        public override Claim? ToClaim() {
            if (ClaimType == null || ClaimValue == null) { return default; }

            var result = new Claim(ClaimType, ClaimValue);
            result.Properties[ID_PROP] = Id.ToString();
            result.Properties[USER_ID_PROP] = UserId.ToString();
            return result;
        }

        public virtual bool Equals(UserClaim? obj) =>
            obj != null &&
            obj.UserId == UserId &&
            obj.ClaimType == ClaimType &&
            obj.ClaimValue == ClaimValue;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserClaim);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserId.GetHashCode() * 7;
                hash += (ClaimType ?? string.Empty).GetHashCode() * 7;
                hash += (ClaimValue ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}
