using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nameless.Web.HealthCheck;

/// <summary>
///     Options for health checks.
/// </summary>
public class HealthCheckRegistration {
    private readonly Dictionary<Type, HealthCheckEntry> _healthChecks = [];

    /// <summary>
    ///     Gets the registered health checks.
    /// </summary>
    public IReadOnlyCollection<HealthCheckEntry> HealthChecks => _healthChecks.Values;

    /// <summary>
    ///     Registers a health check with the specified options.
    /// </summary>
    /// <remarks>
    ///     Health checks implementations are registered as singletons.
    /// </remarks>
    /// <typeparam name="THealthCheck">
    ///     Type of the health check.
    /// </typeparam>
    /// <param name="name">
    ///     The name of the health check.
    /// </param>
    /// <param name="failureStatus">
    ///     The failure status.
    /// </param>
    /// <param name="tags">
    ///     The tags.
    /// </param>
    /// <param name="timeout">
    ///     The timeout for the health check.
    /// </param>
    /// <returns>
    ///     The current <see cref="HealthCheckRegistration"/> instance so other
    ///     actions can be chained.
    /// </returns>
    public HealthCheckRegistration WithHealthCheck<THealthCheck>(string? name = null, HealthStatus? failureStatus = null, string[]? tags = null, TimeSpan? timeout = null)
        where THealthCheck : class, IHealthCheck {
        var type = typeof(THealthCheck);

        _healthChecks[type] = new HealthCheckEntry {
            Name = name ?? type.Name,
            Type = type,
            FailureStatus = failureStatus,
            Tags = tags,
            Timeout = timeout
        };

        return this;
    }
}