using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Configuration;
using Nameless.Web.Security;

namespace Nameless.Web.Hosting.Features;

/// <summary>
///     Registers data protection, configured through
///     <see cref="AppDataProtectionOptions"/>.
/// </summary>
public class DataProtectionFeature : WebHostFeature {
    /// <inheritdoc />
    public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
        var section = ConfigurationSectionNameAttribute.GetSectionName<AppDataProtectionOptions>();
        var options = builder.Configuration
                             .GetSection<AppDataProtectionOptions>()
                             .Get<AppDataProtectionOptions>() ??
                      throw new MissingConfigurationException(section);

        var dataProtection = builder.Services
                                    .AddDataProtection(opts => opts.ApplicationDiscriminator = options.ApplicationDiscriminator)
                                    .SetApplicationName(builder.Environment.ApplicationName);

        if (options.UseFileSystem && !string.IsNullOrWhiteSpace(options.FileSystemPath)) {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(options.FileSystemPath));
        }
    }
}
