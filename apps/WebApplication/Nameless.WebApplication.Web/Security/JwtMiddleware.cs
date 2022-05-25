using Nameless.WebApplication.Web.Services;

namespace Nameless.WebApplication.Web.Security {

    public sealed class JwtMiddleware {

        #region Private Read-Only Fields

        private readonly RequestDelegate? _next;

        #endregion

        #region Public Constructors

        public JwtMiddleware(RequestDelegate? next) {
            Prevent.Null(next, nameof(next));

            _next = next;
        }

        #endregion

        #region Public Methods

        public async Task Invoke(HttpContext context, IUserService userService, IJwtService jwtService) {
            var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null) {
                var token = authorizationHeader.Split(' ').Last();
                var value = jwtService.DecodeToken(token);
                if (value != null) {
                    // attach user to context on successful jwt validation
                    context.Items["User"] = await userService.GetByEmailAsync(value);
                }
            }

            await _next!(context);
        }

        #endregion
    }
}
