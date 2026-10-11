using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace Nameless.Web.RateLimiter;

[RateLimiterPolicy(Policies.RateLimiter.SlidingWindow)]
public class DefaultSlidingWindowRateLimiterPolicy : IRateLimiterPolicy<string> {
    private readonly SlidingWindowRateLimiterOptions _options;

    /// <inheritdoc />
    public Func<OnRejectedContext, CancellationToken, ValueTask> OnRejected => RejectAsync;

    /// <summary>
    ///     Initializes a new instance of
    ///     <see cref="DefaultSlidingWindowRateLimiterPolicy"/> class.
    /// </summary>
    public DefaultSlidingWindowRateLimiterPolicy() {
        _options = new SlidingWindowRateLimiterOptions {
            PermitLimit = 10,                   // max requests
            Window = TimeSpan.FromSeconds(60),  // full window
            SegmentsPerWindow = 4,              // sliding window segments
            QueueLimit = 0,                     // do not queue requests
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        };
    }

    /// <inheritdoc />
    public RateLimitPartition<string> GetPartition(HttpContext httpContext) {
        // partition key: typically user identity name or IP
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "UNKNOWN";

        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => _options);
    }

    private static async ValueTask RejectAsync(OnRejectedContext context, CancellationToken cancellationToken) {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsync(
            text: "Too many requests. Please try again later.",
            cancellationToken
        );
    }
}