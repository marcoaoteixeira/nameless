using Microsoft.AspNetCore.Builder;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Uses the HTTPS redirection middleware.
/// </summary>
public class HttpsRedirectionFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseHttpsRedirection();
    }
}
