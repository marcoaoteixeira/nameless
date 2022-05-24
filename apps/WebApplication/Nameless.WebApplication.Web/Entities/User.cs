namespace Nameless.WebApplication.Web.Entities {

    public class User : EntityBase {

        #region Public Virtual Properties

        public virtual string? Username { get; set; }
        public virtual string? Email { get; set; }
        public virtual string? PasswordHash { get; set; }
        public virtual string? Role { get; set; }

        #endregion

        #region Public Constructors

        public User() : base(Guid.NewGuid()) { }

        #endregion

        #region Public Override Methods

        public override string ToString() => $"{Username} ({Email})";

        #endregion
    }
}
