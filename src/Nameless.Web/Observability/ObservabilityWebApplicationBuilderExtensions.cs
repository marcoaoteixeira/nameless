using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nameless.Helpers;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Nameless.Web.Observability;

/// <summary>
///     Host application builder extension methods.
/// </summary>
public static class ObservabilityWebApplicationBuilderExtensions {
    /// <param name="self">
    ///     The current instance of <see cref="WebApplicationBuilder"/>.
    /// </param>
    extension(WebApplicationBuilder self) {
        /// <summary>
        ///     Registers OpenTelemetry services in the application builder.
        /// </summary>
        /// <remarks>
        ///     <see cref="OpenTelemetryBuilderOtlpExporterExtensions.UseOtlpExporter(IOpenTelemetryBuilder)"/> will
        ///     be used if there is a configuration value for <see cref="StaticData.OpenTelemetry.ExporterEndpointConfigKey" />
        ///     defined.
        /// </remarks>
        /// <param name="configure">
        ///     The registration settings delegate.
        /// </param>
        /// <returns>
        ///     The current <see cref="WebApplicationBuilder"/>
        ///     instance so other actions can be chained.
        /// </returns>
        /// <remarks>
        ///     To add gRPC instrumentation for OpenTelemetry
        ///     Check: https://learn.microsoft.com/en-us/aspnet/core/grpc/diagnostics
        /// </remarks>
        public WebApplicationBuilder RegisterOpenTelemetry(Action<OpenTelemetryRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.SetOpenTelemetryLogger(registration);
            self.SetOpenTelemetry(registration);

            return self;
        }

        private void SetOpenTelemetryLogger(OpenTelemetryRegistration registration) {
            if (!registration.UseOpenTelemetryLogging) { return; }

            self.Logging.AddOpenTelemetry(registration.ConfigureOpenTelemetryLogging ?? (options => {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            }));
        }

        private void SetOpenTelemetry(OpenTelemetryRegistration registration) {
            var builder = self.Services.AddOpenTelemetry();

            builder
                .WithMetrics(metrics => {
                    // Insert metrics for library
                    metrics.AddMeter(typeof(Nameless.AssemblyMarker).Assembly.GetName().Name!);
                    metrics.AddMeter(typeof(AssemblyMarker).Assembly.GetName().Name!);

                    if (registration.OverwriteMeterProviderConfiguration) {
                        registration.ConfigureMeterProvider?.Invoke(metrics);

                        return;
                    }

                    metrics.AddMeter([.. registration.Meters]);
                    metrics.AddHttpClientInstrumentation();
                    metrics.AddAspNetCoreInstrumentation();
                    metrics.AddRuntimeInstrumentation(registration.ConfigureRuntimeInstrumentation);

                    registration.ConfigureMeterProvider?.Invoke(metrics);
                })
                .WithTracing(tracing => {
                    // Insert tracing for library
                    tracing.AddSource(typeof(Nameless.AssemblyMarker).Assembly.GetName().Name!);
                    tracing.AddSource(typeof(AssemblyMarker).Assembly.GetName().Name!);

                    if (registration.OverwriteTracerProviderConfiguration) {
                        registration.ConfigureTracerProvider?.Invoke(tracing);

                        return;
                    }

                    tracing.AddSource([.. registration.TracingSources]);
                    tracing.AddHttpClientInstrumentation(registration.ConfigureHttpClientTraceInstrumentation);
                    tracing.AddAspNetCoreInstrumentation(registration.AspNetCoreTracerName, registration.ConfigureAspNetCoreTraceInstrumentation);

                    registration.ConfigureTracerProvider?.Invoke(tracing);
                })
                .ConfigureResource(registration.ConfigureResources);

            var exporterEndpoint = self.Configuration[StaticData.OpenTelemetry.ExporterEndpointConfigKey];
            if (!string.IsNullOrWhiteSpace(exporterEndpoint)) {
                builder.UseOtlpExporter();
            }
        }
    }
}