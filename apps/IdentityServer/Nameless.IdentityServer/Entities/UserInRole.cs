namespace Nameless.IdentityServer.Entities {

    public class UserInRole : EntityBase {

        #region Public Virtual Properties

        public virtual Guid UserID { get; set; }
        public virtual Guid RoleID { get; set; }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(UserInRole? obj) {
            return obj != null &&
                obj.UserID == UserID &&
                obj.RoleID == RoleID;
        }

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as UserInRole);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += UserID.GetHashCode() * 7;
                hash += RoleID.GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}
