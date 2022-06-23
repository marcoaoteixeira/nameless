namespace Nameless.IdentityServer.Entities {

    public class UserLogin : EntityBase {

        #region Public Virtual Properties

        public virtual Guid UserID { get; set; }
        public virtual string? LoginProvider { get; set; }
        public virtual string? ProviderKey { get; set; }
        public virtual string? DisplayName { get; set; }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(UserLogin? obj) {
            return obj != null &&
                obj.UserID == UserID &&
                obj.LoginProvider == LoginProvider &&
                obj.ProviderKey == ProviderKey;
        }

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserLogin);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserID.GetHashCode() * 7;
                hash += (LoginProvider ?? string.Empty).GetHashCode() * 7;
                hash += (ProviderKey ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}