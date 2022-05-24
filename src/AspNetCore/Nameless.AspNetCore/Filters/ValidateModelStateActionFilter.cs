using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Nameless.AspNetCore.Filters {

    public sealed class ValidateModelStateActionFilter : ActionFilterAttribute {

        #region Public Override Methods

        public override void OnActionExecuting(ActionExecutingContext context) {
            if (!context.ModelState.IsValid) {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        #endregion
    }
}
