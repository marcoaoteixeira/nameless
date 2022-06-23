namespace Nameless.IdentityServer.Entities {

    public class Role : EntityBase {

        #region Private Read-Only Fields

        private readonly Guid _id;
#pragma warning disable 0649
        private readonly int _version;
#pragma warning restore 0649

        #endregion

        #region Public Virtual Properties

        public virtual Guid ID => _id;
        public virtual string? Name { get; set; }
        public virtual int Version => _version;

        #endregion

        #region Public Constructors

        public Role() : this(Guid.NewGuid()) { }

        public Role(Guid id) {
            _id = id == Guid.Empty ? Guid.NewGuid() : id;
        }

        #endregion

        #region Public Virtual Methods

        public virtual bool Equals(Role? obj) => obj != null && obj.ID == ID;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as Role);

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
