using Microsoft.AspNetCore.Builder;
using Nameless.Web.OpenApi;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers OpenAPI and maps its endpoint.
/// </summary>
public class OpenApiFeature : WebHostFeature<OpenApiRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        // Captured now, like every other feature, so a later Configure call
        // has no effect.
        var configure = ConfigureAction;

        builder.Services.RegisterOpenApi(registration => {
            ConfigureDefaults(registration);
            configure?.Invoke(registration);
        });
    }

    /// <inheritdoc />
    public override void Use(WebApplication app, WebHostFeatureContext context) {
        app.MapOpenApi();
    }

    private static void ConfigureDefaults(OpenApiRegistration registration) {
        registration.RegisterOpenApiDocument(
            document: "v1",
            options: options => options.AddDocumentTransformer<BearerSecuritySchemeDocumentTransformer>()
        );
    }
}
