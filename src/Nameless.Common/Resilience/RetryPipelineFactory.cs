using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Nameless.Resilience;

/// <summary>
///     Default implementation of <see cref="IRetryPipelineFactory"/> that builds Polly
///     retry pipelines from a <see cref="RetryPolicyConfiguration"/>.
/// </summary>
public class RetryPipelineFactory : IRetryPipelineFactory {
    private readonly ILogger<RetryPipelineFactory> _logger;

    /// <summary>
    ///     Initializes a new <see cref="RetryPipelineFactory"/>.
    /// </summary>
    /// <param name="logger">Logger used to emit retry warning events.</param>
    public RetryPipelineFactory(ILogger<RetryPipelineFactory> logger) {
        _logger = logger;
    }

    /// <inheritdoc />
    public IRetryPipeline Create(RetryPolicyConfiguration configuration) {
        return configuration.RetryCount > 0
            ? CreateRetryPipeline(configuration)
            : RetryPipeline.Empty;
    }

    private RetryPipeline CreateRetryPipeline(RetryPolicyConfiguration configuration) {
        var strategy = CreateRetryStrategy(configuration);
        var resiliencePipeline = new ResiliencePipelineBuilder().AddRetry(strategy).Build();

        return new RetryPipeline(configuration.Tag, resiliencePipeline);
    }

    private RetryStrategyOptions CreateRetryStrategy(RetryPolicyConfiguration configuration) {
        return new RetryStrategyOptions {
            MaxRetryAttempts = configuration.RetryCount,

            ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => {
                // We do not retry on OperationCanceledException.
                if (ex is OperationCanceledException) { return false; }

                return configuration.RetryOnException is not null &&
                       configuration.RetryOnException(ex);
            }),

            DelayGenerator = args => {
                var delay = CalculateDelay(configuration, args.AttemptNumber);

                if (configuration.UseJitter) {
                    var jitter = Random.Shared.NextDouble() * 0.3; // ±30%
                    var jitterMultiplier = 1.0 + (jitter - 0.15);

                    delay = TimeSpan.FromMilliseconds(
                        delay.TotalMilliseconds * jitterMultiplier
                    );
                }

                // Ensure we do not go over maximum delay.
                if (delay > configuration.MaxDelay) {
                    delay = configuration.MaxDelay;
                }

                return ValueTask.FromResult<TimeSpan?>(delay);
            },

            OnRetry = args => {
                Log.WarningOnRetry(
                    _logger,
                    configuration.Tag,
                    args.AttemptNumber,
                    configuration.RetryCount,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Exception
                );

                configuration.OnRetry?.Invoke(
                    args.Outcome.Exception,
                    args.RetryDelay,
                    args.AttemptNumber,
                    configuration.RetryCount
                );

                return ValueTask.CompletedTask;
            }
        };
    }

    private static TimeSpan CalculateDelay(RetryPolicyConfiguration configuration, int currentAttempt) {
        return configuration.BackoffType switch {
            BackoffType.Linear => TimeSpan.FromMilliseconds(
                configuration.InitialDelay.TotalMilliseconds * currentAttempt
            ),

            BackoffType.Exponential => TimeSpan.FromMilliseconds(
                configuration.InitialDelay.TotalMilliseconds * Math.Pow(2, currentAttempt - 1)
            ),

            _ => configuration.InitialDelay
        };
    }
}
