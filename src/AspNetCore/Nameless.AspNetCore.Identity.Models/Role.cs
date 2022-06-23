using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class Role : IdentityRole<Guid> {

        #region Public Constructors

        public Role() {
            Id = Guid.NewGuid();
        }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(Role? obj) => obj != null && obj.Id == Id;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as Role);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += Id.GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}
