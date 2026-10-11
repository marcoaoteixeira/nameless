using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Configuration;

namespace Nameless;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/> instance.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Configures the specified options type by binding it to a
        ///     configuration section with a name derived from the type,
        ///     using the provided configuration source.
        /// </summary>
        /// <remarks>
        ///     The section name is determined using the
        ///     <see cref="ConfigurationSectionNameAttribute"/> applied to the
        ///     options type, or the type name if the attribute is not
        ///     present. This method simplifies options binding by
        ///     automatically selecting the appropriate configuration section.
        /// </remarks>
        /// <typeparam name="TOptions">
        ///     The options class to configure. Must be a reference type.
        /// </typeparam>
        /// <param name="configuration">
        ///     The configuration source to use for binding. If
        ///     <see langword="null"/>, the options type is registered with
        ///     default values. If a configuration section is provided, it is
        ///     used directly; otherwise, the section matching the options type
        ///     name is retrieved from the configuration root.
        /// </param>
        /// <returns>
        ///     The current instance of <see cref="IServiceCollection"/> so
        ///     other actions can be chained.
        /// </returns>
        public IServiceCollection ConfigureOptions<TOptions>(IConfiguration? configuration = null) where TOptions : class {
            // solve it fast, if null them apply the empty delegate
            if (configuration is null) {
                return self.Configure<TOptions>(_ => { });
            }

            var sectionName = ConfigurationSectionNameAttribute.GetSectionName<TOptions>();

            return configuration is IConfigurationSection section && section.Key == sectionName
                ? self.Configure<TOptions>(section)
                : self.Configure<TOptions>(configuration.GetSection(sectionName));
        }
    }
}
