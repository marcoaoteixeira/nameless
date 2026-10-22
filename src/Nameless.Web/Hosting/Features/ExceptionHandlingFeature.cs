using Microsoft.AspNetCore.Builder;
using Nameless.Helpers;
using Nameless.Web.ErrorHandling;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the error handling services and uses the exception handler middleware.
/// </summary>
public class ExceptionHandlingFeature : WebHostFeature<ErrorHandlingRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterErrorHandling(ConfigureAction);
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        var registration = ActionHelper.FromDelegate(ConfigureAction);
        var opts = ActionHelper.FromDelegate(registration.ConfigureExceptionHandlerOptions);

        app.UseExceptionHandler(opts);
    }
}
