using Microsoft.Extensions.Hosting;
using Nameless.Registration;
using Nameless.Validation.FluentValidation;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

/// <summary>
///     Registers the validator, scanning the context assemblies.
/// </summary>
public class ValidationFeature : WinHostFeature<FluentValidationValidatorRegistration> {
    /// <inheritdoc />
    public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
        var configure = ConfigureAction;
        var assemblies = context.Assemblies;

        builder.ConfigureServices(
            services => services.RegisterValidator(
                AssemblyScanAwareHelper.MergeAssemblies(configure, assemblies)
            )
        );
    }
}
