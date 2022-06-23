using System.Security.Claims;

namespace Nameless.IdentityServer.Entities {

    public class UserClaim : EntityBase {

        #region Public Virtual Properties

        public virtual Guid UserID { get; set; }
        public virtual string? Type { get; set; }
        public virtual string? Value { get; set; }

        #endregion

        #region Public Virtual Methods

        public virtual Claim? ToClaim() {
            if (Type == null || Value == null) { return null; }

            var result = new Claim(Type, Value);
            result.Properties[nameof(UserID)] = UserID.ToString();
            return result;
        }

        public virtual bool Equals(UserClaim? obj) =>
            obj != null &&
            obj.UserID == UserID &&
            obj.Type == Type &&
            obj.Value == Value;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserClaim);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserID.GetHashCode() * 7;
                hash += (Type ?? string.Empty).GetHashCode() * 7;
                hash += (Value ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}
