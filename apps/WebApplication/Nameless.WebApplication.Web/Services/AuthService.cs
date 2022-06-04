using Nameless.WebApplication.Web.Utils;

namespace Nameless.WebApplication.Web.Services {

    public sealed class AuthService : IAuthService {

        #region Private Read-Only Fields

        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtService _jwtService;

        #endregion

        #region Public Constructors

        public AuthService(IUserService userService, IRefreshTokenService refreshTokenService, IJwtService jwtService) {
            Prevent.Null(userService, nameof(userService));
            Prevent.Null(refreshTokenService, nameof(refreshTokenService));
            Prevent.Null(jwtService, nameof(jwtService));

            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _jwtService = jwtService;
        }

        #endregion

        #region IAuthService Members

        public async Task<AuthenticationResult> AuthenticateAsync(string identification, string? password, string ipAddress, CancellationToken cancellationToken = default) {
            Prevent.NullEmptyOrWhiteSpace(identification, nameof(identification));
            Prevent.NullEmptyOrWhiteSpace(password, nameof(password));

            var user = await _userService.GetByEmailAsync(identification, cancellationToken);
            if (user == null || Password.Validate(password, user.PasswordHash!)) {
                return AuthenticationResult.Fail("Username or password is incorrect.");
            }
            var jwtToken = _jwtService.GenerateToken(user.Email!);
            var refreshToken = await _refreshTokenService.CreateAsync(user, ipAddress, cancellationToken);
            if (refreshToken == null) {
                return AuthenticationResult.Fail("Could not generate refresh token.");
            }
            await _refreshTokenService.RemoveExpiredTokensAsync(cancellationToken);

            return AuthenticationResult.Success(user.Username!, user.Email!, jwtToken, refreshToken.Token!, refreshToken.ExpiresDate);
        }

        #endregion
    }
}
