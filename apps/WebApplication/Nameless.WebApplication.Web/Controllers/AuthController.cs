using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nameless.AspNetCore.Extensions;
using Nameless.WebApplication.Web.Models;
using Nameless.WebApplication.Web.Services;

namespace Nameless.WebApplication.Web.Controllers {

    public sealed class AuthController : AppControllerBase {

        #region Private Read-Only Fields

        private readonly IAuthService _authService;
        private readonly ICacheService _cacheService;

        #endregion

        #region Public Constructors

        public AuthController(IAuthService authService, ICacheService cacheService) {
            Prevent.Null(authService, nameof(authService));
            Prevent.Null(cacheService, nameof(cacheService));

            _authService = authService;
            _cacheService = cacheService;
        }

        #endregion

        #region Public Methods

        [AllowAnonymous, HttpPost]
        public async Task<IActionResult> Authenticate(AuthenticationRequest request) {
            var response = await _authService.AuthenticateAsync(request.Identification!, request.Password!, HttpContext.GetIpAddress());

            if(!response.Successfull) {
                ModelState.AddModelError(string.Empty, response.Error!);
                return BadRequest(ModelState);
            }

            await _cacheService.StoreAsync(
                key: request.Identification!,
                value: response.RefreshToken!,
                expiration: new DateTimeOffset(response.RefreshTokenExpiration)
            );
            
            return Ok();
        }

        #endregion
    }
}
