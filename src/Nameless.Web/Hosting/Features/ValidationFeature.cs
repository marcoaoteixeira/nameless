using Microsoft.AspNetCore.Builder;
using Nameless.Registration;
using Nameless.Validation.FluentValidation;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers the validator, scanning the context assemblies.
/// </summary>
public class ValidationFeature : WebHostFeature<FluentValidationValidatorRegistration> {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        builder.Services.RegisterValidator(
            AssemblyScanAwareHelper.MergeAssemblies(ConfigureAction, context.Assemblies)
        );
    }
}
