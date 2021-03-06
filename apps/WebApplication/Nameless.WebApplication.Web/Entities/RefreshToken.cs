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

        public RefreshToken() : base(Guid.NewGuid()) { }
        public RefreshToken(DateTime? creationDate = null) : base(Guid.NewGuid(), creationDate) { }

        #endregion

        #region Public Methods

        public virtual bool IsExpired(IClock? clock = null) => (clock ?? SystemClock.Instance).UtcNow >= ExpiresDate;
        public virtual bool IsRevoked() => RevokedDate != null;
        public virtual bool IsActive(IClock? clock = null) => !IsRevoked() && !IsExpired(clock);

        #endregion
    }
}
