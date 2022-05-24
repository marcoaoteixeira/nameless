using Microsoft.AspNetCore.Mvc;

namespace Nameless.WebApplication.Web {

    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public abstract class AppControllerBase : ControllerBase {

    }
}
