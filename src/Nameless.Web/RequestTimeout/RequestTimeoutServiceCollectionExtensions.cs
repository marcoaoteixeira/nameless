using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Web.Configuration;

namespace Nameless.Web.RequestTimeout;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class RequestTimeoutServiceCollectionExtensions {
    extension(IServiceCollection self) {
        /// <summary>
        ///     Configures request timeout service with predefined expiration
        ///     policies for the application.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <param name="includeDefaultPolicies">
        ///     Whether it should include predefined policies.
        /// </param>
        /// <remarks>
        ///     This method adds request timeout to the application's service
        ///     collection and registers some defaults policies for
        ///     one-second, five-seconds, fifteen-seconds, thirty-seconds and
        ///     one-minute expirations. For more information about request
        ///     timeout in ASP.NET Core, see <a href="https://learn.microsoft.com/en-us/aspnet/core/performance/timeouts">Request Timeout middleware in ASP.NET Core</a>
        /// </remarks>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> instance so
        ///     other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterRequestTimeout(IConfiguration? configuration = null, bool includeDefaultPolicies = false) {
            self.AddRequestTimeouts(builder => {
                var policies = configuration.GetPolicies<RequestTimeoutPolicyOptions>();

                if (includeDefaultPolicies) { IncludeDefaultPolicies(policies); }

                foreach (var policy in policies.Where(item => !item.Value.Skip)) {
                    builder.AddPolicy(
                        policy.Key,
                        policy.Value.ToPolicy()
                    );
                }
            });

            return self;
        }
    }

    private static void IncludeDefaultPolicies(Dictionary<string, RequestTimeoutPolicyOptions> actual) {
        actual[Policies.RequestTimeout.OneSecond] = new RequestTimeoutPolicyOptions {
            ExpiresIn = TimeSpan.FromSeconds(1),
            HttpStatusCode = StatusCodes.Status408RequestTimeout,
            Skip = false
        };

        actual[Policies.RequestTimeout.FiveSeconds] = new RequestTimeoutPolicyOptions {
            ExpiresIn = TimeSpan.FromSeconds(5),
            HttpStatusCode = StatusCodes.Status408RequestTimeout,
            Skip = false
        };

        actual[Policies.RequestTimeout.FifteenSeconds] = new RequestTimeoutPolicyOptions {
            ExpiresIn = TimeSpan.FromSeconds(15),
            HttpStatusCode = StatusCodes.Status408RequestTimeout,
            Skip = false
        };

        actual[Policies.RequestTimeout.ThirtySeconds] = new RequestTimeoutPolicyOptions {
            ExpiresIn = TimeSpan.FromSeconds(30),
            HttpStatusCode = StatusCodes.Status408RequestTimeout,
            Skip = false
        };

        actual[Policies.RequestTimeout.OneMinute] = new RequestTimeoutPolicyOptions {
            ExpiresIn = TimeSpan.FromMinutes(1),
            HttpStatusCode = StatusCodes.Status408RequestTimeout,
            Skip = false
        };
    }
}