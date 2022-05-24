using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web.Services {

    public interface IRefreshTokenService {

        #region Methods

        Task<RefreshToken?> CreateAsync(User user, string ipAddress, CancellationToken cancellationToken = default);
        Task RemoveExpiredTokensAsync(CancellationToken cancellationToken = default);

        #endregion
    }
}
