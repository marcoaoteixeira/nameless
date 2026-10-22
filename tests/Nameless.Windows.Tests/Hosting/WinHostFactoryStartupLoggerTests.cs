using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting;

[CollectionDefinition(nameof(ConsoleCaptureCollection), DisableParallelization = true)]
public class ConsoleCaptureCollection;

/// <summary>
///     The startup logger writes to the console, which is global state, so
///     these tests redirect <see cref="Console.Out"/> and must not run in
///     parallel with other tests.
/// </summary>
[IntegrationTest]
[Collection(nameof(ConsoleCaptureCollection))]
public class WinHostFactoryStartupLoggerTests {
    public class ThrowingFeature(Exception exception) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) => throw exception;
    }

    public class LoggingProbeFeature(params (LogLevel Level, string Message)[] entries) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
            foreach (var (level, message) in entries) {
                context.Logger.Log(level, message);
            }
        }
    }

    private static string CaptureConsole(Action action) {
        var original = Console.Out;
        using var writer = new StringWriter();

        Console.SetOut(writer);

        try {
            action();
        } finally {
            Console.SetOut(original);
        }

        return writer.ToString();
    }

    [Fact]
    public void WhenCreateConsole_WhenFeatureThrows_ThenLogCritical() {
        // act
        var actual = CaptureConsole(() => Assert.Throws<InvalidOperationException>(() => WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            features.Add(new ThrowingFeature(new InvalidOperationException("boom")));
        })));

        // assert
        Assert.Multiple(
            () => Assert.Contains("FTL", actual),
            () => Assert.Contains("Windows host failed to build.", actual),
            () => Assert.Contains("boom", actual)
        );
    }

    [Fact]
    public void WhenCreateConsole_WhenMinimumLevelIsConfigured_ThenStartupLoggerHonoursIt() {
        // act
        var actual = CaptureConsole(() => {
            using var host = WinHostFactory.CreateConsole(
                args: ["--Serilog:MinimumLevel=Warning"],
                assemblies: [],
                configureFeatures: features => features.Add(new LoggingProbeFeature(
                    (LogLevel.Information, "information-probe"),
                    (LogLevel.Warning, "warning-probe")
                ))
            );
        });

        // assert
        Assert.Multiple(
            () => Assert.DoesNotContain("information-probe", actual),
            () => Assert.Contains("warning-probe", actual)
        );
    }

    [Fact]
    public void WhenCreateConsole_WhenSinksAreConfigured_ThenStartupLoggerIgnoresThem() {
        // arrange
        const string probe = "sink-probe";

        // act
        var actual = CaptureConsole(() => {
            using var host = WinHostFactory.CreateConsole(
                // Without the filter, this sink would duplicate every entry.
                args: ["--Serilog:WriteTo:0:Name=Console"],
                assemblies: [],
                configureFeatures: features => features.Add(new LoggingProbeFeature((LogLevel.Warning, probe)))
            );
        });

        // assert
        Assert.Single(actual.Split(probe).Skip(1));
    }

    [Fact]
    public void WhenCreateConsole_WhenHostIsAborted_ThenRethrowWithoutLogging() {
        // act
        var actual = CaptureConsole(() => Assert.Throws<HostAbortedException>(() => WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            features.Add(new ThrowingFeature(new HostAbortedException()));
        })));

        // assert
        Assert.DoesNotContain("Windows host failed to build.", actual);
    }
}
