using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nameless.Windows.Hosting.Internals;
using Nameless.Windows.Hosting.Wrappers;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;

namespace Nameless.Windows.Hosting;

/// <summary>
///     Creates the Windows host from an ordered
///     <see cref="WinHostFeatureCollection"/>.
/// </summary>
/// <remarks>
///     The host configuration (<c>AppSettings.json</c>,
///     <c>AppSettings.{environment}.json</c> and environment variables) is
///     mandatory and always applied by the factory, so it is not a feature.
/// </remarks>
public sealed class WinHostFactory {
    private const string DEFAULT_ENVIRONMENT = "Development";

    private readonly string[] _args;
    private readonly string _environment;
    private readonly Assembly[] _assemblies;
    private readonly WinHostFeatureCollection _features;

    private WinHostFactory(string[] args, string environment, Assembly[] assemblies, WinHostFeatureCollection features) {
        _args = args;
        _environment = environment;
        _assemblies = assemblies;
        _features = features;
    }

    /// <summary>
    ///     Creates and builds the host for a Windows application, from
    ///     <see cref="WinHostFeatureCollection.CreateWinAppBase"/>.
    /// </summary>
    /// <param name="args">
    ///     The command-line arguments. If <see langword="null"/>, no
    ///     arguments are used.
    /// </param>
    /// <param name="assemblies">
    ///     The assemblies to scan for types, available to every feature
    ///     through <see cref="WinHostFeatureContext.Assemblies"/>.
    ///     Nameless.Common and Nameless.Windows are always scanned, first.
    ///     If <see langword="null"/>, the assembly that called this method
    ///     is scanned too. Pass the assemblies explicitly when calling from
    ///     a shared library that wraps this method.
    /// </param>
    /// <param name="environment">
    ///     The environment name used to load
    ///     <c>AppSettings.{environment}.json</c>. If <see langword="null"/>
    ///     or white space, <c>Development</c> is used.
    /// </param>
    /// <param name="configureFeatures">
    ///     An optional delegate that configures the ordered collection of
    ///     features. Use it to add, remove, replace or configure features.
    ///     Application services belong in an application feature added
    ///     last with <see cref="WinHostFeatureCollection.Add"/>: its
    ///     <see cref="WinHostFeature.Register"/> runs after every other
    ///     registration, so it can replace them.
    /// </param>
    /// <returns>
    ///     The built <see cref="WinHost"/>. It is not started.
    /// </returns>
    // No inlining: GetCallingAssembly must see the caller of this method.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static WinHost CreateWinApp(string[]? args = null, IEnumerable<Assembly>? assemblies = null, string? environment = null, Action<WinHostFeatureCollection>? configureFeatures = null) {
        return CreateFrom(
            args,
            assemblies ?? [Assembly.GetCallingAssembly()],
            environment,
            WinHostFeatureCollection.CreateWinAppBase(),
            configureFeatures
        );
    }

    /// <summary>
    ///     Creates and builds the host for a console application, from
    ///     <see cref="WinHostFeatureCollection.CreateConsoleBase"/>.
    /// </summary>
    /// <inheritdoc cref="CreateWinApp" path="/param"/>
    /// <inheritdoc cref="CreateWinApp" path="/returns"/>
    // No inlining: GetCallingAssembly must see the caller of this method.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static WinHost CreateConsole(string[]? args = null, IEnumerable<Assembly>? assemblies = null, string? environment = null, Action<WinHostFeatureCollection>? configureFeatures = null) {
        return CreateFrom(
            args,
            assemblies ?? [Assembly.GetCallingAssembly()],
            environment,
            WinHostFeatureCollection.CreateConsoleBase(),
            configureFeatures
        );
    }

    private static WinHost CreateFrom(string[]? args, IEnumerable<Assembly> assemblies, string? environment, WinHostFeatureCollection features, Action<WinHostFeatureCollection>? configureFeatures) {
        configureFeatures?.Invoke(features);

        // Library assemblies first: their built-ins are found by assembly scan.
        Assembly[] supportAssemblies = [
            .. assemblies.Prepend(typeof(AssemblyMarkerWindows).Assembly)
                         .Prepend(typeof(AssemblyMarker).Assembly)
                         .Distinct()
        ];

        return new WinHostFactory(
            args ?? [],
            string.IsNullOrWhiteSpace(environment) ? DEFAULT_ENVIRONMENT : environment,
            supportAssemblies,
            features
        ).Build();
    }

    private WinHost Build() {
        // The startup logger is local on purpose: assigning it to the static
        // Log.Logger would break processes that build more than one host.
        // It reads only the minimum level from configuration; configured
        // sinks belong to the application logger.
        using var serilog = CreateSerilogLogger();
        using var loggerFactory = new SerilogLoggerFactory(serilog);

        var context = new WinHostFeatureContext(
            assemblies: _assemblies,
            logger: loggerFactory.CreateLogger(typeof(WinHostFactory).FullName!)
        );

        try {
            var builder = Host.CreateDefaultBuilder(_args)
                              .ConfigureHostConfiguration(config => ConfigureHostConfiguration(config, reloadOnChange: true))
                              .Wrap();

            // Snapshot, so a feature cannot change the collection being iterated.
            foreach (var feature in _features.ToArray()) {
                Log.RegisteringFeature(context.Logger, feature.Name);

                feature.Register(builder, context);
            }

            return builder.Build().Wrap();
        }
        // Tools like "dotnet ef" abort the host on purpose; that's not a failure.
        catch (Exception exception) when (exception is not HostAbortedException) {
            Log.HostBuildFailure(
                context.Logger,
                $"Windows host failed to build. Reason: {exception.Message}",
                exception
            );

            throw;
        }
    }

    private void ConfigureHostConfiguration(IConfigurationBuilder config, bool reloadOnChange) {
        config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile(path: "AppSettings.json", optional: true, reloadOnChange)
              .AddJsonFile(path: $"AppSettings.{_environment}.json", optional: true, reloadOnChange)
              .AddEnvironmentVariables();
    }

    private Logger CreateSerilogLogger() {
        var hostConfiguration = new ConfigurationBuilder();

        // Read once and discarded: no file watchers must outlive this call.
        ConfigureHostConfiguration(hostConfiguration, reloadOnChange: false);

        // Application configuration also receives the command-line arguments.
        var configuration = new ConfigurationBuilder()
            .AddConfiguration(hostConfiguration.Build())
            .AddCommandLine(_args)
            .Build();

        // Keys keep their full path ("Serilog:MinimumLevel:..."), so Serilog
        // reads them as if they came from the original configuration.
        var minimumLevel = new ConfigurationBuilder()
            .AddInMemoryCollection(
                configuration.GetSection("Serilog:MinimumLevel").AsEnumerable()
            )
            .Build();

        return new LoggerConfiguration()
            .ReadFrom.Configuration(minimumLevel)
            .WriteTo.Console()
            .CreateLogger();
    }
}
