namespace Nameless.Resilience;

/// <summary>
///     <see cref="IRetryPipelineFactory"/> extension methods.
/// </summary>
public static class RetryPipelineFactoryExtensions {
    /// <param name="self">
    ///     The current <see cref="IRetryPipelineFactory"/> instance.
    /// </param>
    extension(IRetryPipelineFactory self) {
        /// <summary>
        ///     Creates a <see cref="IRetryPipeline"/> with default parameters.
        /// </summary>
        /// <param name="onRetry">
        ///     The delegate to execute on retry.
        /// </param>
        /// <returns>
        ///     A new instance of <see cref="IRetryPipeline"/>.
        /// </returns>
        public IRetryPipeline CreateDefault(Action<Exception?, TimeSpan, int, int>? onRetry = null) {
            return self.Create(
                configuration: RetryPolicyConfiguration.CreateDefault(onRetry)
            );
        }
    }
}
