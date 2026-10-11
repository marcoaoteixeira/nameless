using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nameless.Helpers;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Nameless.Logging.Serilog;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods for Serilog logging integration.
/// </summary>
public static class SerilogServiceCollectionExtensions {
    /// <param name="self">The current <see cref="IServiceCollection"/>.</param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers Serilog as the logging provider, applying either the default
        ///     configuration (console, file, OpenTelemetry) or a custom override.
        /// </summary>
        /// <param name="configure">Optional delegate to configure Serilog options.</param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions can be chained.
        /// </returns>
        public IServiceCollection RegisterSerilog(Action<SerilogRegistration>? configure) {
            var registration = ActionHelper.FromDelegate(configure);

            self.AddSerilog((provider, config) => {
                ConfigureSettings(provider, config.ReadFrom, registration);
                ConfigureEnrichment(provider, config.Enrich, registration);
                ConfigureSink(provider, config.WriteTo, registration);
                ConfigureMinimumLevel(provider, config.MinimumLevel, registration);
            });

            return self.AddLogging(
                builder => builder.AddSerilog()
            );
        }
    }

    private static void ConfigureSettings(IServiceProvider provider, LoggerSettingsConfiguration config, SerilogRegistration registration) {
        if (!registration.OverwriteSettingsConfiguration) {
            // Defines from where it should get its configurations.
            config.Configuration(
                provider.GetRequiredService<IConfiguration>(),
                readerOptions: null
            );
        }

        registration.ConfigureSettings?.Invoke(provider, config);
    }

    private static void ConfigureEnrichment(IServiceProvider provider, LoggerEnrichmentConfiguration config, SerilogRegistration registration) {
        if (!registration.OverwriteEnrichmentConfiguration) {
            // Enrich the log message with data from other locations.
            config.FromLogContext();
        }

        registration.ConfigureEnrichment?.Invoke(provider, config);
    }

    private static void ConfigureSink(IServiceProvider provider, LoggerSinkConfiguration config, SerilogRegistration registration) {
        if (!registration.OverwriteSinkConfiguration) {
            // Enrich the log message with data from other locations.
            // Write to console sink
            config.Console();

            // Write to file sink
            config.File(
                path: "app-.log",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 16_777_216,
                retainedFileCountLimit: 3
            );

            // Write to OpenTelemetry sink
            var configuration = provider.GetRequiredService<IConfiguration>();
            var exporterUrl = configuration[StaticData.OpenTelemetry.ExporterEndpointConfigKey];
            if (!string.IsNullOrWhiteSpace(exporterUrl)) {
                config.OpenTelemetry(opts => opts.Endpoint = exporterUrl);
            }
        }

        registration.ConfigureSink?.Invoke(provider, config);
    }

    private static void ConfigureMinimumLevel(IServiceProvider provider, LoggerMinimumLevelConfiguration config, SerilogRegistration registration) {
        if (!registration.OverwriteMinimumLevelConfiguration) {
            // Enrich the log message with data from other locations.
            config.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning);
            config.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning);
            config.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning);
        }

        registration.ConfigureMinimumLevel?.Invoke(provider, config);
    }
}
