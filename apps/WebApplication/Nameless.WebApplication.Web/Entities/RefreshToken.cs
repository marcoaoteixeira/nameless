using Nameless.Services;

namespace Nameless.WebApplication.Web.Entities {

    public class RefreshToken : EntityBase {

        #region Public Virtual Properties

        public virtual Guid UserId { get; set; }
        public virtual string? Token { get; set; }
        public virtual DateTime ExpiresDate { get; set; }
        public virtual string? CreatedByIp { get; set; }
        public virtual DateTime? RevokedDate { get; set; }
        public virtual string? RevokedByIp { get; set; }
        public virtual string? ReplacedByToken { get; set; }
        public virtual string? ReasonRevoked { get; set; }

        #endregion

        #region Public Constructors

        public RefreshToken(DateTime? creationDate = null) : base(Guid.NewGuid(), creationDate) { }

        #endregion

        #region Public Methods

        public bool IsExpired(IClock? clock = null) => (clock ?? SystemClock.Instance).UtcNow >= ExpiresDate;
        public bool IsRevoked() => RevokedDate != null;
        public bool IsActive(IClock? clock = null) => !IsRevoked() && !IsExpired(clock);

        #endregion
    }
}
