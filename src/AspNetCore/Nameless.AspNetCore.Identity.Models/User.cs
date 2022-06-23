using Microsoft.AspNetCore.Identity;

namespace Nameless.AspNetCore.Identity.Models {

    public class User : IdentityUser<Guid> {

        #region Public Virtual Properties

        public virtual string? FirstName { get; set; }
        public virtual string? LastName { get; set; }
        public virtual string? AvatarUrl { get; set; }

        #endregion

        #region Public Constructors

        public User() {
            Id = Guid.NewGuid();
        }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(User? obj) => obj != null && obj.Id == Id;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as User);

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