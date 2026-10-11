using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Nameless.Diagnostics.CodeAnalysis;

namespace Nameless.Web.Scalar.Auth;

// Why? => https://learn.microsoft.com/en-us/dotnet/core/extensions/logging/high-performance-logging
[ExcludeFromCodeCoverage(Justification = CodeCoverage.Justifications.AutoGenCode)]
internal static partial class Log {
    [LoggerMessage(LogLevel.Information, message: "[{Tag}] Scalar HTTP Security Scheme failed to retrieve the access token. Reason: {Reason}")]
    internal static partial void UnableRetrieveAccessToken(ILogger logger, string reason, string? tag = null);
}
