using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.Resilience;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods for resilience services.
/// </summary>
public static class ResilienceServiceCollectionExtensions {
    /// <param name="self">The current <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the <see cref="IRetryPipelineFactory"/> as a transient service.
        /// </summary>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterResilience() {
            self.TryAddTransient<IRetryPipelineFactory, RetryPipelineFactory>();

            return self;
        }
    }
}
