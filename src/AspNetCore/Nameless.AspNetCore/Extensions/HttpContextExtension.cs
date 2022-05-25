using Microsoft.AspNetCore.Http;

namespace Nameless.AspNetCore.Extensions {

    public static class HttpContextExtension {

        #region Public Static Methods

        public static string GetIpAddress(this HttpContext self) {
            Prevent.Null(self, nameof(self));

            if (self.Request.Headers.ContainsKey("X-Forwarded-For")) {
                return self.Request.Headers["X-Forwarded-For"];
            }

            if (self.Connection.RemoteIpAddress == null) {
                return string.Empty;
            }

            return self.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        #endregion
    }
}
