using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Nameless.Web.Hosting;

[CollectionDefinition(nameof(ConsoleCaptureCollection), DisableParallelization = true)]
public class ConsoleCaptureCollection;

/// <summary>
///     The startup logger writes to the console, which is global state, so
///     these tests redirect <see cref="Console.Out"/> and must not run in
///     parallel with other tests.
/// </summary>
[IntegrationTest]
[Collection(nameof(ConsoleCaptureCollection))]
public class WebHostFactoryStartupLoggerTests
{
    public class ThrowingFeature(Exception exception) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => throw exception;
    }

    public class LoggingFeature(params (LogLevel Level, string Message)[] entries) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context)
        {
            foreach (var (level, message) in entries)
            {
                context.Logger.Log(level, message);
            }
        }
    }

    private static string CaptureConsole(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();

        Console.SetOut(writer);

        try
        {
            action();
        }
        finally
        {
            Console.SetOut(original);
        }

        return writer.ToString();
    }

    [Fact]
    public void WhenCreate_WhenFeatureThrows_ThenLogCritical()
    {
        // act
        var actual = CaptureConsole(() => Assert.Throws<InvalidOperationException>(() => WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            features.Add(new ThrowingFeature(new InvalidOperationException("boom")));
        })));

        // assert
        Assert.Multiple(
            () => Assert.Contains("FTL", actual),
            () => Assert.Contains("Web host failed to build.", actual),
            () => Assert.Contains("boom", actual)
        );
    }

    [Fact]
    public void WhenCreate_WhenMinimumLevelIsConfigured_ThenStartupLoggerHonoursIt()
    {
        // act
        var actual = CaptureConsole(() =>
        {
            using var host = WebHostFactory.Create(
                args: [.. TestHostSettings.Required, "--Serilog:MinimumLevel=Warning"],
                assemblies: [],
                configureFeatures: features => features.Add(new LoggingFeature(
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
    public void WhenCreate_WhenSinksAreConfigured_ThenStartupLoggerIgnoresThem()
    {
        // arrange
        const string probe = "sink-probe";

        // act
        var actual = CaptureConsole(() =>
        {
            using var host = WebHostFactory.Create(
                // Without the filter, this sink would duplicate every entry.
                args: [.. TestHostSettings.Required, "--Serilog:WriteTo:0:Name=Console"],
                assemblies: [],
                configureFeatures: features => features.Add(new LoggingFeature((LogLevel.Warning, probe)))
            );
        });

        // assert
        Assert.Single(actual.Split(probe).Skip(1));
    }

    [Fact]
    public void WhenCreate_WhenHostIsAborted_ThenRethrowWithoutLogging()
    {
        // act
        var actual = CaptureConsole(() => Assert.Throws<HostAbortedException>(() => WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            features.Add(new ThrowingFeature(new HostAbortedException()));
        })));

        // assert
        Assert.DoesNotContain("Web host failed to build.", actual);
    }
}
