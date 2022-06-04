using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Nameless.Services;
using Nameless.WebApplication.Web.Security;

namespace Nameless.WebApplication.Web.Services {

    public class JWTService : IJwtService {

        #region Private Read-Only Fields

        private readonly IClock _clock;
        private readonly JwtOptions _opts;

        #endregion

        #region Public Constructors

        public JWTService(IClock clock, JwtOptions opts) {
            _clock = clock ?? SystemClock.Instance;
            _opts = opts ?? JwtOptions.Default;
        }

        #endregion

        #region IJwtService Members

        public string DecodeToken(string token) {
            if (token == null) { return string.Empty; }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_opts.Secret!);

            tokenHandler.ValidateToken(token, new() {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,

                // set clockskew to zero so tokens expire exactly 
                // at token expiration time, instead of 5 minutes later
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken securityToken);

            var jwt = (JwtSecurityToken)securityToken;
            var claim = jwt.Claims.FirstOrDefault(_ => _.Type == ClaimTypes.Email);

            return claim != null ? claim.Value : string.Empty;
        }

        public string GenerateToken(string email) {
            if (string.IsNullOrWhiteSpace(email)) { return string.Empty; }

            // generate token that is valid for N seconds
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_opts.Secret!);
            var claims = new[] { new Claim(ClaimTypes.Email, email) };
            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(claims),
                Expires = _clock.UtcNow.AddSeconds(_opts.RefreshTokenTTL),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        #endregion
    }
}
