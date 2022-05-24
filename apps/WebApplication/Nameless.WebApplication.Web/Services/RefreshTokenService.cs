using System.Security.Cryptography;
using Nameless.Services;
using Nameless.WebApplication.Web.Entities;
using Nameless.WebApplication.Web.Persistence;
using Nameless.WebApplication.Web.Security;

namespace Nameless.WebApplication.Web.Services {

    public sealed class RefreshTokenService : IRefreshTokenService {

        #region Private Read-Only Fields

        private readonly IRepository _repository;
        private readonly IClock _clock;
        private readonly JwtOptions _opts;

        #endregion

        #region Public Constructors

        public RefreshTokenService(IRepository repository, IClock clock, JwtOptions opts) {
            Ensure.NotNull(repository, nameof(repository));

            _repository = repository;
            _clock = clock ?? SystemClock.Instance;
            _opts = opts ?? JwtOptions.Default;
        }

        #endregion

        #region IRefreshTokenService Members

        public Task<RefreshToken?> CreateAsync(User user, string ipAddress, CancellationToken cancellationToken = default) {
            Ensure.NotNull(user, nameof(user));
            Ensure.NotNullEmptyOrWhiteSpace(ipAddress, nameof(ipAddress));

            var refreshToken = new RefreshToken(_clock.UtcNow) {
                UserId = user.Id,
                Token = getUniqueToken(),
                ExpiresDate = _clock.UtcNow.AddSeconds(_opts.RefreshTokenTTL),
                CreatedByIp = ipAddress
            };

            return _repository.SaveAsync(refreshToken, cancellationToken);

            string getUniqueToken() {
                // token is a cryptographically strong random sequence of values
                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                // ensure token is unique by checking against db
                var exists = _repository.Exists<RefreshToken>(_ => _.Token == token);

                return exists ? token : getUniqueToken();
            }
        }

        public Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default) {
            var utcNow = _clock.UtcNow;
            var ttl = _opts.RefreshTokenTTL;

            return _repository.DeleteAllAsync<RefreshToken>(
                filter: _ => _.RevokedDate != null
                          && _.ExpiresDate > utcNow
                          && _.CreationDate.AddSeconds(ttl) <= utcNow,
                cancellationToken: cancellationToken
            );
        }

        #endregion
    }
}
