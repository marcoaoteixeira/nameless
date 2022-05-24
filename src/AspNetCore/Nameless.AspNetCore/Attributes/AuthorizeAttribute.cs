using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nameless;

namespace Nameless.AspNetCore.Attributes {

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AuthorizeAttribute : Attribute, IAuthorizationFilter {

        #region Public Static Read-Only Fields

        public static readonly string UserHttpContextKey = "User";

        #endregion

        #region IAuthorizationFilter Members

        public void OnAuthorization(AuthorizationFilterContext context) {

            var allowAnonymous = context
                .ActionDescriptor
                .EndpointMetadata.OfType<AllowAnonymousAttribute>()
                .Any();

            if (allowAnonymous) { return; }

            var hasUser = context.HttpContext.Items.ContainsKey(UserHttpContextKey);
            if (hasUser) {
                context.Result = new JsonResult(new { message = "Unauthorized" }) {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

        }

        #endregion
    }
}
