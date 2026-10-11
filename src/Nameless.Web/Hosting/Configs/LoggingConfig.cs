using Microsoft.AspNetCore.Builder;
using Nameless.Logging.Serilog;
using Nameless.Web.Serilog;
using Serilog;

namespace Nameless.Web.Hosting.Configs;

public static class LoggingConfig {
    extension(WebApplicationBuilder self) {
        /// <summary>
        ///     Configures Serilog-based logging for the application, including
        ///     console and OpenTelemetry sinks, and enriches log events with
        ///     contextual information.
        /// </summary>
        /// <remarks>
        ///     This method sets up Serilog as the logging provider, reading
        ///     configuration from the application's configuration sources. It
        ///     enriches log events with contextual data such as correlation IDs
        ///     and log context, and writes logs to both the console and
        ///     OpenTelemetry endpoints if configured. Minimum log levels for
        ///     common ASP.NET Core components are overridden to reduce log
        ///     verbosity.
        /// </remarks>
        /// <returns>
        ///     The current <see cref="WebApplicationBuilder"/> instance so
        ///     other actions can be chained.
        /// </returns>
        public WebApplicationBuilder ConfigureLogging(WebHostSettings settings) {
            if (settings.DisableLogging) { return self; }

            self.Services.RegisterSerilog(
                settings.ConfigureSerilog ?? DefaultSerilogConfiguration
            );

            return self;
        }
    }

    private static void DefaultSerilogConfiguration(SerilogRegistration registration) {
        registration.WithEnrichmentConfiguration((provider, enrich) => {
            // Enrich with request CorrelationId
            enrich.WithCorrelationId(provider);
        });
    }

    /// <param name="self">
    ///     The current <see cref="WebApplication"/> instance.
    /// </param>
    extension(WebApplication self) {
        /// <summary>
        ///     Uses the logging service.
        /// </summary>
        /// <param name="settings">
        ///     The <see cref="WebHostSettings"/> instance.
        /// </param>
        /// <returns>
        ///     The current <see cref="WebApplication"/> so other actions
        ///     can be chained.
        /// </returns>
        public WebApplication UseLogging(WebHostSettings settings) {
            if (settings.DisableLogging) { return self; }

            self.UseSerilogRequestLogging();

            return self;
        }
    }
}