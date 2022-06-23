namespace Nameless.IdentityServer.Entities {

    public class User : EntityBase {

        #region Private Read-Only Fields

        private readonly Guid _id;
#pragma warning disable 0649
        private readonly int _version;
#pragma warning restore 0649

        #endregion

        #region Public Virtual Properties

        public virtual Guid ID => _id;
        public virtual string? Username { get; set; }
        public virtual string? FirstName { get; set; }
        public virtual string? LastName { get; set; }
        public virtual string? Email { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual string? PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual string? PasswordHash { get; set; }
        public virtual string? AvatarUrl { get; set; }
        public virtual bool TwoFactorAuthEnabled { get; set; }
        public virtual DateTime? LockoutEnd { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual int AccessFailureCounter { get; set; }
        public virtual int Version => _version;

        #endregion

        #region Public Constructors

        public User() : this(Guid.NewGuid()) { }
        public User(Guid id) {
            _id = id == Guid.Empty ? Guid.NewGuid() : id;
        }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(User? obj) => obj != null && obj.ID == ID;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as User);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += ID.GetHashCode() * 7;
            }
            return hash;
        }

        #endregion
    }
}