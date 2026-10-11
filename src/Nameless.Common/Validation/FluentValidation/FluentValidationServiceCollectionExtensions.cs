using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;

namespace Nameless.Validation.FluentValidation;

/// <summary>
///     <see cref="IServiceCollection" /> extension methods.
/// </summary>
public static class FluentValidationServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection" />.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the validation services.
        /// </summary>
        /// <param name="configure">
        ///     The registration settings delegate.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection" /> so other actions
        ///     can be chained.
        /// </returns>
        public IServiceCollection RegisterValidator(Action<FluentValidationValidatorRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.TryAddTransient<IValidator, FluentValidationValidator>();
            self.TryAddEnumerable(registration.Validators.Select(
                implementation => ServiceDescriptor.Transient(
                    typeof(IFluentValidationValidator),
                    implementation
                )
            ));

            return self;
        }
    }
}