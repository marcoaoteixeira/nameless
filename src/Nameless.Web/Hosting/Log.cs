using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Nameless.Web.Hosting;

/// <summary>
///     Why? => https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/high-performance-logging
/// </summary>
[ExcludeFromCodeCoverage]
internal static partial class Log {
    [LoggerMessage(LogLevel.Information, message: "Registering feature: {Feature}")]
    internal static partial void RegisteringFeature(ILogger logger, string feature);

    [LoggerMessage(LogLevel.Information, message: "Using feature: {Feature}")]
    internal static partial void UsingFeature(ILogger logger, string feature);

    [LoggerMessage(LogLevel.Critical, message: "{Message}")]
    internal static partial void BuildFailure(ILogger logger, string message, Exception ex);
}
