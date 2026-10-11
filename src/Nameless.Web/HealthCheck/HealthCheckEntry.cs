using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Nameless.Web.HealthCheck;

/// <summary>
///     Represents a health check registration options.
/// </summary>
public record HealthCheckEntry {
    /// <summary>
    ///     Gets the health check name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the health check type.
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    ///     Gets the health check failure status.
    /// </summary>
    public HealthStatus? FailureStatus { get; init; }

    /// <summary>
    ///     Gets the health check tags.
    /// </summary>
    public string[]? Tags { get; init; }

    /// <summary>
    ///     Gets the health check timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}