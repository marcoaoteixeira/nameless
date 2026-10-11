using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Instrumentation.Runtime;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nameless.Web.Observability;

/// <summary>
///     OpenTelemetry options for configuring logging and tracing in the application.
/// </summary>
public class OpenTelemetryRegistration {
    private readonly HashSet<string> _meters = [];
    private readonly HashSet<string> _tracingSources = [];

    /// <summary>
    ///     Whether it should add OpenTelemetry to the logger.
    /// </summary>
    public bool UseOpenTelemetryLogging { get; private set; }

    /// <summary>
    ///     Gets or sets the action to configure OpenTelemetry logger
    ///     options.
    /// </summary>
    public Action<OpenTelemetryLoggerOptions>? ConfigureOpenTelemetryLogging { get; private set; }

    /// <summary>
    ///     Gets or sets the collection of meters names used for
    ///     diagnostic logging.
    /// </summary>
    public IReadOnlyCollection<string> Meters => _meters;

    /// <summary>
    ///     Whether it should overwrite the meter provider configuration.
    /// </summary>
    /// <remarks>
    ///     The default behavior is as follows: the meter provider has a
    ///     default configuration, and when
    ///     <see cref="ConfigureMeterProvider"/> is defined, it will extend
    ///     the current configuration. When
    ///     <see cref="OverwriteMeterProviderConfiguration"/> is set to
    ///     <see langword="true"/>, the <see cref="ConfigureMeterProvider"/>
    ///     delegate will overwrite the default configuration instead.
    ///     Important: <see cref="ConfigureRuntimeInstrumentation"/> will be
    ///     overwritten, if <see cref="OverwriteMeterProviderConfiguration"/> is
    ///     set to <see langword="true"/>.
    /// </remarks>
    public bool OverwriteMeterProviderConfiguration { get; private set; }

    /// <summary>
    ///     Gets or sets a delegate that configures the meter provider.
    /// </summary>
    public Action<MeterProviderBuilder>? ConfigureMeterProvider { get; private set; }

    /// <summary>
    ///     Gets or sets the collection of tracing source names used for
    ///     diagnostic logging.
    /// </summary>
    public IReadOnlyCollection<string> TracingSources => _tracingSources;

    /// <summary>
    ///     Whether it should overwrite the meter provider configuration.
    /// </summary>
    /// <remarks>
    ///     The default behavior is as follows: the meter provider has a
    ///     default configuration, and when
    ///     <see cref="ConfigureTracerProvider"/> is defined, it will extend
    ///     the current configuration. When
    ///     <see cref="OverwriteTracerProviderConfiguration"/> is set to
    ///     <see langword="true"/>, the <see cref="ConfigureTracerProvider"/>
    ///     delegate will overwrite the default configuration instead.
    ///     Important: <see cref="ConfigureHttpClientTraceInstrumentation"/> and
    ///     <see cref="ConfigureAspNetCoreTraceInstrumentation"/> will be
    ///     overwritten, if <see cref="OverwriteTracerProviderConfiguration"/> is
    ///     set to <see langword="true"/>.
    /// </remarks>
    public bool OverwriteTracerProviderConfiguration { get; private set; }

    /// <summary>
    ///     Gets or sets a delegate that will extend the current OpenTelemetry
    ///     tracer provider configuration.
    /// </summary>
    public Action<TracerProviderBuilder>? ConfigureTracerProvider { get; private set; }

    /// <summary>
    ///     Gets or sets the action to configure HTTP client trace
    ///     instrumentation options.
    /// </summary>
    public Action<RuntimeInstrumentationOptions>? ConfigureRuntimeInstrumentation { get; private set; }

    /// <summary>
    ///     Gets or sets the action to configure HTTP client trace
    ///     instrumentation options.
    /// </summary>
    public Action<HttpClientTraceInstrumentationOptions>? ConfigureHttpClientTraceInstrumentation { get; private set; }

    /// <summary>
    ///     Gets or sets the ASP.NET Core tracer name.
    /// </summary>
    public string? AspNetCoreTracerName { get; private set; }

    /// <summary>
    ///     Gets or sets the action to configure ASP.NET Core trace
    ///     instrumentation options.
    /// </summary>
    public Action<AspNetCoreTraceInstrumentationOptions>? ConfigureAspNetCoreTraceInstrumentation { get; private set; }

    /// <summary>
    ///     Gets or sets the action to configure OpenTelemetry resource
    ///     builder.
    /// </summary>
    public Action<ResourceBuilder> ConfigureResources { get; private set; } = _ => { };

    /// <summary>
    ///     Sets whether it should add OpenTelemetry log category.
    /// </summary>
    /// <param name="value">
    ///     The value
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithUseOpenTelemetryLogging(bool value) {
        UseOpenTelemetryLogging = value;

        return this;
    }

    /// <summary>
    ///     Sets OpenTelemetry logging configuration.
    /// </summary>
    /// <remarks>
    ///     It overwrites the default configuration.
    /// </remarks>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithOpenTelemetryLoggingConfiguration(Action<OpenTelemetryLoggerOptions> configure) {
        ConfigureOpenTelemetryLogging = configure;

        return this;
    }

    /// <summary>
    ///     Adds the specified meter.
    /// </summary>
    /// <remarks>
    ///     If <see cref="WithMeterProviderConfiguration"/> sets overwrite
    ///     to <see langword="true"/>, the meter defined by this method
    ///     are also overwritten.
    /// </remarks>
    /// <param name="value">
    ///     The meter name.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithMeter(string value) {
        Throws.When.NullOrWhiteSpace(value);

        _meters.Add(value);

        return this;
    }

    /// <summary>
    ///     Sets the meter provider configuration action.
    /// </summary>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <param name="overwrite">
    ///     Whether it should overwrite the default configuration.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithMeterProviderConfiguration(Action<MeterProviderBuilder> configure, bool overwrite = false) {
        ConfigureMeterProvider = configure;
        OverwriteMeterProviderConfiguration = overwrite;

        return this;
    }

    /// <summary>
    ///     Adds the specified tracing source.
    /// </summary>
    /// <remarks>
    ///     If <see cref="WithTracingConfiguration"/> sets overwrite
    ///     to <see langword="true"/>, the tracing sources defined by
    ///     this method are also overwritten.
    /// </remarks>
    /// <param name="value">
    ///     The tracing source.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithTracingSource(string value) {
        Throws.When.NullOrWhiteSpace(value);

        _tracingSources.Add(value);

        return this;
    }

    /// <summary>
    ///     Sets the tracing source configuration.
    /// </summary>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <param name="overwrite">
    ///     Whether it should overwrite the default configuration.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithTracingConfiguration(Action<TracerProviderBuilder> configure, bool overwrite = false) {
        ConfigureTracerProvider = configure;
        OverwriteTracerProviderConfiguration = overwrite;

        return this;
    }

    /// <summary>
    ///     Configures .NET runtime instrumentation.
    /// </summary>
    /// <remarks>
    ///     If <see cref="WithMeterProviderConfiguration"/> sets overwrite
    ///     to <see langword="true"/>, the runtime instrumentation
    ///     configuration defined by this method will be also overwritten.
    /// </remarks>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithRuntimeInstrumentation(Action<RuntimeInstrumentationOptions> configure) {
        ConfigureRuntimeInstrumentation = configure;

        return this;
    }

    /// <summary>
    ///     Configures HTTP client tracing instrumentation.
    /// </summary>
    /// <remarks>
    ///     If <see cref="WithTracingConfiguration"/> sets overwrite
    ///     to <see langword="true"/>, the configuration defined by this
    ///     method will be also overwritten.
    /// </remarks>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithHttpClientTraceInstrumentation(Action<HttpClientTraceInstrumentationOptions> configure) {
        ConfigureHttpClientTraceInstrumentation = configure;

        return this;
    }

    /// <summary>
    ///     Sets the ASPNET Core tracer name.
    /// </summary>
    /// <remarks>
    ///     If <see cref="WithTracingConfiguration"/> sets overwrite
    ///     to <see langword="true"/>, the configuration defined by this
    ///     method will be also overwritten.
    /// </remarks>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithAspNetTracerName(string value) {
        Throws.When.NullOrWhiteSpace(value);

        AspNetCoreTracerName = value;

        return this;
    }

    /// <summary>
    ///     Configures the ASPNET Core tracing instrumentation.
    /// </summary>
    /// <remarks>
    ///     If <see cref="WithTracingConfiguration"/> sets overwrite
    ///     to <see langword="true"/>, the configuration defined by this
    ///     method will be also overwritten.
    /// </remarks>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithAspNetCoreTraceInstrumentation(Action<AspNetCoreTraceInstrumentationOptions> configure) {
        ConfigureAspNetCoreTraceInstrumentation = configure;

        return this;
    }

    /// <summary>
    ///     Configures the resources.
    /// </summary>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenTelemetryRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public OpenTelemetryRegistration WithResourceConfiguration(Action<ResourceBuilder> configure) {
        ConfigureResources = configure;

        return this;
    }
}