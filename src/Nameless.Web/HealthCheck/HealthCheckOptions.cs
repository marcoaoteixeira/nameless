using Nameless.Configuration;

namespace Nameless.Web.HealthCheck;

/// <summary>
///     Health check configuration options.
/// </summary>
[ConfigurationSectionName("HealthCheck")]
public record HealthCheckOptions {
    /// <summary>
    ///     Gets or sets the liveness route path. Default is <c>health</c>.
    /// </summary>
    public string LivenessPath { get; set; } = "health";

    /// <summary>
    ///     Gets or sets the readiness route path. Default is <c>ready</c>.
    /// </summary>
    public string ReadinessPath { get; set; } = "ready";

    /// <summary>
    ///     Gets or sets the readiness tags.
    /// </summary>
    public string[] ReadinessTags { get; set; } = [];
}