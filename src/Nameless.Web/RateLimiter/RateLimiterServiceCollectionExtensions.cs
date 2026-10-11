using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Helpers;

namespace Nameless.Web.RateLimiter;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class RateLimiterServiceCollectionExtensions {
    extension(IServiceCollection self) {
        /// <summary>
        ///     Configures rate limiting for the application using a sliding
        ///     window limiter with predefined settings.
        /// </summary>
        /// <param name="configure">
        ///     The registration delegate.
        /// </param>
        /// <param name="includeDefaultPolicies">
        ///     Whether it should use the predefined policies.
        /// </param>
        /// <remarks>
        ///     This method adds a sliding window rate limiter to the
        ///     application's service collection, limiting requests to 10 per
        ///     60 seconds, divided into 4 segments. Requests exceeding the
        ///     limit receive a 429 Too Many Requests response.
        ///     For more information on rate limiting in ASP.NET Core, see
        ///     <a href="https://learn.microsoft.com/aspnet/core/performance/rate-limit">Rate limiting middleware in ASP.NET Core</a>
        /// </remarks>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> instance so
        ///     other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterRateLimiter(Action<RateLimiterRegistration>? configure = null, bool includeDefaultPolicies = false) {
            var registration = ActionHelper.FromDelegate(configure);

            var policies = new Dictionary<string, Type>(registration.RateLimiterPolicies);

            if (includeDefaultPolicies) {
                IncludeDefaultPolicies(policies);
            }

            self.AddRateLimiter(builder => {
                foreach (var kvp in policies) {
                    builder.RegisterPolicy(kvp.Key, kvp.Value);
                }
            });

            return self;
        }
    }

    private static void IncludeDefaultPolicies(Dictionary<string, Type> actual) {
        actual[Policies.RateLimiter.SlidingWindow] = typeof(DefaultSlidingWindowRateLimiterPolicy);
    }
}