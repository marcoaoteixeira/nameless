using Microsoft.AspNetCore.Builder;
using Nameless.Web.Auth;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers authentication and authorization, and uses their middlewares.
/// </summary>
public class AuthFeature : WebHostFeature<AuthRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterAuth(ConfigureAction, builder.Configuration);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseAuthentication();
        app.UseAuthorization();
    }
}
