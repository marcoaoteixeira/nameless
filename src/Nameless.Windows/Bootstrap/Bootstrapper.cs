using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Nameless.Reporting;
using Nameless.Resilience;
using Nameless.Windows.Bootstrap.Execution;
using Nameless.Windows.Bootstrap.Notification;

namespace Nameless.Windows.Bootstrap;

/// <summary>
///     Default implementation of <see cref="IBootstrapper"/> that executes
///     bootstrap steps sequentially.
/// </summary>
public class Bootstrapper : IBootstrapper, IDisposable {
    private readonly IStep[] _steps;
    private readonly IRetryPipelineFactory _retryPipelineFactory;
    private readonly TimeProvider _timeProvider;
    private readonly IStatusReporter<Bootstrapper> _statusReporter;
    private readonly ILogger<Bootstrapper> _logger;
    private readonly IProgress<StepProgress> _progress;

    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Bootstrapper"/> class.
    /// </summary>
    /// <param name="retryPipelineFactory">
    ///     The retry policy factory.
    /// </param>
    /// <param name="statusReporter">
    ///     The status reporter.
    /// </param>
    /// <param name="steps">
    ///     The collection of steps to be executed during the bootstrap
    ///     process.
    /// </param>
    /// <param name="timeProvider">
    ///     The time provider.
    /// </param>
    /// <param name="logger">
    ///     The logger used to record execution details and diagnostic
    ///     information.
    /// </param>
    public Bootstrapper(IRetryPipelineFactory retryPipelineFactory, IStatusReporter<Bootstrapper> statusReporter, IEnumerable<IStep> steps, TimeProvider timeProvider, ILogger<Bootstrapper> logger) {
        _steps = [.. steps];
        _retryPipelineFactory = retryPipelineFactory;
        _timeProvider = timeProvider;
        _statusReporter = statusReporter;
        _logger = logger;
        _progress = new Progress<StepProgress>(HandleStepProgress);
    }

    /// <summary>
    ///     Destructor.
    /// </summary>
    ~Bootstrapper() {
        Dispose(disposing: false);
    }

    /// <inheritdoc />
    /// <exception cref="BootstrapException">
    ///     if one or more steps fail during execution.
    /// </exception>
    public async Task RunAsync(CancellationToken cancellationToken) {
        BlockAccessAfterDispose();

        var graph = StepExecutionGraphBuilder.Create(_steps);

        _statusReporter.ReportBootstrapperStarting();

        try {
            await ExecuteStepsAsync(graph, cancellationToken).SkipContextSync();

            var failures = graph.GetExecutionResults()
                                .Where(result => !result.Success)
                                .ToArray();

            if (failures.Length > 0) {
                throw new BootstrapException("One or more steps failed.", failures);
            }
        }
        catch (Exception ex) { _statusReporter.ReportBootstrapperFault(ex); throw; }
        finally { _statusReporter.ReportBootstrapperComplete(); }
    }

    /// <inheritdoc />
    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Executes all steps in the graph. Override this method to provide
    ///     a custom execution strategy, such as parallel execution.
    /// </summary>
    /// <param name="graph">The execution graph that defines the order and dependencies of steps.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected virtual async Task ExecuteStepsAsync(StepExecutionGraph graph, CancellationToken cancellationToken) {
        BlockAccessAfterDispose();

        foreach (var level in graph) {
            foreach (var node in level) {
                await ExecuteStepWithRetryAsync(node, cancellationToken).SkipContextSync();
            }
        }
    }

    /// <summary>
    ///     Executes a single step with retry support.
    /// </summary>
    /// <param name="node">The execution node representing the step to run.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task ExecuteStepWithRetryAsync(StepExecutionNode node, CancellationToken cancellationToken) {
        BlockAccessAfterDispose();

        var sw = Stopwatch.StartNew();

        node.Result.StartTime = _timeProvider.GetUtcNow();

        try {
            if (node.Step.IsDisabled) { return; }

            var retryPipeline = CreateRetryPipeline(node.Step);

            await retryPipeline.ExecuteAsync(
                async token => await node.Step
                                         .ExecuteAsync(_progress, token)
                                         .SkipContextSync(),
                cancellationToken
            ).ConfigureAwait(continueOnCapturedContext: false);
        }
        catch (Exception ex) {
            node.Result.Exception = ex;

            CommonLog.Error(_logger, ex.Message, ex, tag: node.Step.DisplayName);

            throw;
        }
        finally { node.Result.Duration = sw.Elapsed; }
    }

    /// <summary>
    ///     Disposes the Bootstrapper.
    /// </summary>
    /// <param name="disposing">
    ///     Whether it should dispose the managed resources.
    /// </param>
    protected virtual void Dispose(bool disposing) {
        if (_disposed) { return; }

        if (disposing) {
            if (_statusReporter is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        _disposed = true;
    }

    private void BlockAccessAfterDispose() {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private IRetryPipeline CreateRetryPipeline(IStep step) {
        if (step.RetryPolicy is null) { return RetryPipeline.Empty; }

        // Add callback to report retry via progress
        var configuration = step.RetryPolicy with {
            Tag = step.RetryPolicy.Tag ?? step.DisplayName,

            // invoke original callback
            OnRetry = step.RetryPolicy.OnRetry
        };

        return _retryPipelineFactory.Create(configuration);
    }

    private void HandleStepProgress(StepProgress progress) {
        _statusReporter.ReportStepProgress(progress);
    }
}
