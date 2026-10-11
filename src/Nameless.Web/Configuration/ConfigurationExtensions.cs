using Microsoft.Extensions.Configuration;
using Nameless.Configuration;

namespace Nameless.Web.Configuration;

/// <summary>
///     <see cref="IConfiguration"/> extension methods.
/// </summary>
internal static class ConfigurationExtensions {
    /// <param name="self">
    ///     The current <see cref="IConfiguration"/> instance.
    /// </param>
    extension(IConfiguration? self) {
        internal Dictionary<string, TPolicy> GetPolicies<TPolicy>() where TPolicy : class {
            return self?
                   .GetSection<TPolicy>()
                   .GetChildren()
                   .ToDictionary(
                       keySelector: item => item.Key,
                       elementSelector: item => item.GetOrThrow<TPolicy>()
                   ) ?? [];
        }
    }
}
