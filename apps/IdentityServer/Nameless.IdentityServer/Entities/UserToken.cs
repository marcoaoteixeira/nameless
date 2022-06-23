namespace Nameless.IdentityServer.Entities {

    public class UserToken : EntityBase {

        #region Public Properties

        public virtual Guid UserID { get; set; }
        public virtual string? LoginProvider { get; set; }
        public virtual string? Name { get; set; }
        public virtual string? Value { get; set; }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(UserToken? obj) {
            return obj != null &&
                obj.UserID == UserID &&
                obj.LoginProvider == LoginProvider &&
                obj.Name == Name;
        }

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserToken);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserID.GetHashCode() * 7;
                hash += (LoginProvider ?? string.Empty).GetHashCode() * 7;
                hash += (Name ?? string.Empty).GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}