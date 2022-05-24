using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nameless.Notification;

namespace Nameless.AspNetCore.Notification {

    public class NotificationActionFilter : IActionFilter {

        #region Public Static Read-Only Fields

        public static readonly string NotifierTempDataKey = "__NOTIFICATION__";

        #endregion

        #region Private Read-Only Fields

        private readonly INotifier _notifier;

        #endregion

        #region Public Constructors

        public NotificationActionFilter(INotifier notifier) {
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        #endregion

        #region IActionFilter Members

        public void OnActionExecuted(ActionExecutedContext context) {
            if (context.Controller is Controller controller) {
                controller.TempData[NotifierTempDataKey] = _notifier.Flush();
            }
        }

        public void OnActionExecuting(ActionExecutingContext context) {

        }

        #endregion
    }
}
