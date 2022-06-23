using Nameless.Services;

namespace Nameless.IdentityServer.Entities {

    public class RefreshToken : EntityBase {

        #region Private Read-Only Fields

        private readonly Guid _id;

        #endregion

        #region Public Virtual Properties

        public virtual Guid ID => _id;
        public virtual Guid UserID { get; set; }
        public virtual string? Token { get; set; }
        public virtual DateTime ExpiresDate { get; set; }
        public virtual string? CreatedByIp { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public virtual string? RevokedByIp { get; set; }
        public virtual DateTime? RevokedDate { get; set; }
        public virtual string? ReplacedByToken { get; set; }
        public virtual string? ReasonRevoked { get; set; }

        #endregion

        #region Public Constructors

        public RefreshToken() : this(Guid.NewGuid()) { }
        public RefreshToken(Guid id) {
            _id = id == Guid.Empty ? Guid.NewGuid() : id;
        }

        #endregion

        #region Public Methods

        public virtual bool IsExpired(IClock? clock = null) => (clock ?? SystemClock.Instance).UtcNow >= ExpiresDate;
        public virtual bool IsRevoked() => RevokedDate != null;
        public virtual bool IsActive(IClock? clock = null) => !IsRevoked() && !IsExpired(clock);

        #endregion
    }
}
