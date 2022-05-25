using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nameless.WebApplication.Web.Controllers {

    public sealed class HomeController : AppControllerBase {

        #region Public Methods

        [AllowAnonymous]
        public IActionResult Get() {
            return Ok("Welcome!");
        }

        #endregion
    }
}
