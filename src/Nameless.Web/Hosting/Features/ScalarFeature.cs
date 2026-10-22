using Microsoft.AspNetCore.Builder;
using Nameless.Web.Scalar;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Maps the Scalar API reference.
/// </summary>
public class ScalarFeature : WebHostFeature<ScalarRegistration> {
    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.UseScalar(ConfigureAction);
    }
}
