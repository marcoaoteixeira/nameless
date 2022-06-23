using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class RoleClaim : IdentityRoleClaim<Guid> {

        #region Private Constants

        private const string ID_PROP = "Id";
        private const string ROLE_ID_PROP = "RoleId";

        #endregion

        #region Public Virtual Methods

        public override void InitializeFromClaim(Claim other) {
            ClaimType = other.Type;
            ClaimValue = other.Value;

            if (other.Properties.TryGetValue(ID_PROP, out var id)) {
                Id = int.Parse(id);
            }
            if (other.Properties.TryGetValue(ROLE_ID_PROP, out var roleId)) {
                RoleId = Guid.Parse(roleId);
            }
        }

        public override Claim? ToClaim() {
            if (ClaimType == null || ClaimValue == null) { return default; }

            var result = new Claim(ClaimType, ClaimValue);
            result.Properties[ID_PROP] = Id.ToString();
            result.Properties[ROLE_ID_PROP] = RoleId.ToString();
            return result;
        }

        public virtual bool Equals(RoleClaim? obj) =>
            obj != null &&
            obj.RoleId == RoleId &&
            obj.ClaimType == ClaimType &&
            obj.ClaimValue == ClaimValue;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as RoleClaim);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += RoleId.GetHashCode() * 7;
                hash += (ClaimType ?? string.Empty).GetHashCode() * 7;
                hash += (ClaimValue ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}
