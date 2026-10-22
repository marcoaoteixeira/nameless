using Microsoft.AspNetCore.Builder;
using Nameless.Registration;
using Nameless.Web.RateLimiter;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the request rate limiter, scanning the context assemblies, and uses its middleware.
/// </summary>
public class RateLimiterFeature : WebHostFeature<RateLimiterRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterRateLimiter(
            AssemblyScanAwareHelper.MergeAssemblies(ConfigureAction, context.Assemblies)
        );
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseRateLimiter();
    }
}
