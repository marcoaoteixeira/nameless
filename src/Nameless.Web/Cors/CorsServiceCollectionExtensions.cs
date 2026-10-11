using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Web.Configuration;

namespace Nameless.Web.Cors;

/// <summary>
///     <see cref="WebApplicationBuilder"/> extension methods.
/// </summary>
public static class CorsServiceCollectionExtensions {
    extension(IServiceCollection self) {
        /// <summary>
        ///     Configures Cross-Origin Resource Sharing (CORS) policies for
        ///     the application.
        /// </summary>
        /// <returns>
        ///     This method register CORS policies using options specified in
        ///     the application's configuration. It adds both custom policies
        ///     and a permissive policy that allows any origin, header, and
        ///     method. Call this method during application configuration to
        ///     enable CORS support for HTTP requests.
        ///     See more at <a href="https://learn.microsoft.com/en-us/aspnet/core/security/cors">Enable Cross-Origin Request (CORS) in ASP.NET Core</a>
        /// </returns>
        public IServiceCollection RegisterCors(IConfiguration? configuration = null, bool includeDefaultPolicies = false) {
            self.AddCors(builder => {
                var policies = configuration.GetPolicies<CorsPolicyOptions>();

                if (includeDefaultPolicies) {
                    IncludeDefaultPolicies(policies);
                }
                
                foreach (var policy in policies.Where(item => !item.Value.Skip)) {
                    builder.AddPolicy(policy.Key, policy.Value.ToPolicy());
                }
            });

            return self;
        }
    }

    private static void IncludeDefaultPolicies(Dictionary<string, CorsPolicyOptions> actual) {
        actual[Policies.Cors.AllowEverything] = new CorsPolicyOptions {
            Headers = "*",
            Methods = "*",
            Origins = "*",
            SupportsCredentials = false,
            PreflightMaxAge = null,
            Skip = false
        };
    }
}