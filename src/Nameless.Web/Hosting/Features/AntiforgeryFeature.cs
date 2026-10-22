using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers antiforgery services, using the <c>X-CSRF-TOKEN</c> header
///     and an HTTP-only, secure-only cookie of the same name, and uses the
///     antiforgery middleware.
/// </summary>
public class AntiforgeryFeature : WebHostFeature<AntiforgeryOptions> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        // Captured now, like every other feature, so a later Configure call
        // has no effect.
        var configure = ConfigureAction;

        builder.Services.AddAntiforgery(opts => {
            ConfigureDefaults(opts);
            configure?.Invoke(opts);
        });
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseAntiforgery();
    }

    private static void ConfigureDefaults(AntiforgeryOptions opts) {
        opts.HeaderName = "X-CSRF-TOKEN";
        opts.Cookie.Name = "X-CSRF-TOKEN";
        opts.Cookie.HttpOnly = true;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    }
}
