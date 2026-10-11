using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Nameless.Web.Correlation;

/// <summary>
///     <see cref="IHostApplicationBuilder"/> extension methods.
/// </summary>
public static class CorrelationWebApplicationExtensions {
    /// <param name="self">
    ///     The current <see cref="WebApplicationBuilder" />.
    /// </param>
    extension(WebApplication self) {
        /// <summary>
        ///     Configures the correlation middleware in the application.
        /// </summary>
        /// <returns>
        ///     The current <see cref="WebApplicationBuilder" /> so other actions
        ///     can be chained.
        /// </returns>
        public WebApplication UseHttpRequestCorrelation() {
            var options = self.Services.GetOptions<HttpRequestCorrelationOptions>();
            
            self.UseMiddleware<HttpRequestCorrelationMiddleware>(options);

            return self;
        }
    }
}