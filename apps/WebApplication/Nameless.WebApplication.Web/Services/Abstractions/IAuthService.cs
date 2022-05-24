namespace Nameless.WebApplication.Web.Services {

    public interface IAuthService {

        #region Methods

        Task<AuthenticationResult> AuthenticateAsync(string identification, string password, string ipAddress, CancellationToken cancellationToken = default);

        #endregion
    }

    public sealed class AuthenticationResult {

        #region Public Properties

        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? JwtToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
        public string? Error { get; set; }
        public bool Successfull => Error == null;

        #endregion

        #region Private Constructors

        private AuthenticationResult(string? error = null) {
            Error = error;
        }

        #endregion

        #region Public Static Methods

        public static AuthenticationResult Success(string username, string email, string jwtToken, string refreshToken, DateTime refreshTokenExpiration) {
            Ensure.NotNullEmptyOrWhiteSpace(username, nameof(username));
            Ensure.NotNullEmptyOrWhiteSpace(email, nameof(email));
            Ensure.NotNullEmptyOrWhiteSpace(jwtToken, nameof(jwtToken));
            Ensure.NotNullEmptyOrWhiteSpace(refreshToken, nameof(refreshToken));
            Ensure.NotNull(refreshTokenExpiration, nameof(refreshTokenExpiration));

            return new AuthenticationResult {
                Username = username,
                Email = email,
                JwtToken = jwtToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiration = refreshTokenExpiration
            };
        }

        public static AuthenticationResult Fail(string? error = null) {
            return new AuthenticationResult(error ?? "An error has occurred.");
        }

        #endregion
    }
}
