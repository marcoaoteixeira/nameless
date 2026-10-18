using Polly;

namespace Nameless.Resilience;

/// <summary>
///     Default implementation of <see cref="IRetryPipeline"/> that wraps a Polly
///     <see cref="ResiliencePipeline"/> to execute operations with retry semantics.
/// </summary>
public class RetryPipeline : IRetryPipeline {
    private readonly ResiliencePipeline _inner;

    /// <summary>
    ///     Gets a no-op <see cref="RetryPipeline"/> that executes operations without any retry.
    /// </summary>
    public static RetryPipeline Empty => new(tag: $"{Guid.CreateVersion7():N}", ResiliencePipeline.Empty);

    /// <summary>
    ///     Gets the optional tag used to identify this pipeline in logs and telemetry.
    /// </summary>
    public string? Tag { get; }

    /// <summary>
    ///     Initializes a new <see cref="RetryPipeline"/>.
    /// </summary>
    /// <param name="tag">An optional identifier for this pipeline.</param>
    /// <param name="inner">The underlying Polly <see cref="ResiliencePipeline"/>.</param>
    public RetryPipeline(string? tag, ResiliencePipeline inner) {
        _inner = inner;

        Tag = tag;
    }

    /// <inheritdoc />
    public ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> operation, CancellationToken cancellationToken) {
        return _inner.ExecuteAsync(operation, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> operation, CancellationToken cancellationToken) {
        return _inner.ExecuteAsync(operation, cancellationToken);
    }
}
