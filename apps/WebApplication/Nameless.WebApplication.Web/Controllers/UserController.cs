using Microsoft.AspNetCore.Mvc;

namespace Nameless.WebApplication.Web.Controllers {

    public sealed class UserController : AppControllerBase {

        [HttpGet]
        public IActionResult Get() {
            return Ok("ok");
        }
    }
}
