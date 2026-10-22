using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Nameless.Web.Hosting;

[IntegrationTest]
public class WebHostFactoryTests
{
    // Not this test assembly: scanning it registers fixture handlers whose
    // dependencies are missing, which fails the host build in Development
    // (ValidateOnBuild). For the same reason, hosts built here pass
    // "assemblies: []" (Nameless only) instead of relying on the
    // calling-assembly convention.
    private static readonly Assembly[] ScanFreeAssemblies = [typeof(Assert).Assembly];

    private static readonly Assembly[] LibraryAssemblies = [typeof(Nameless.AssemblyMarker).Assembly, typeof(AssemblyMarker).Assembly];

    public class FirstRecordingFeature(List<string> steps) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => steps.Add("register:first");

        public override void Use(WebApplication app, WebHostFeatureContext context) => steps.Add("use:first");
    }

    public class SecondRecordingFeature(List<string> steps) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => steps.Add("register:second");

        public override void Use(WebApplication app, WebHostFeatureContext context) => steps.Add("use:second");
    }

    public class ContextCapturingFeature : WebHostFeature
    {
        public WebHostFeatureContext? Captured { get; private set; }

        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => Captured = context;
    }

    public class ThrowingFeature(Exception exception) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => throw exception;
    }

    // Aborts before the application is built, so the assemblies in the
    // context are captured without being validated by the container.
    public class AbortingContextCapturingFeature : WebHostFeature
    {
        public WebHostFeatureContext? Captured { get; private set; }

        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context)
        {
            Captured = context;

            throw new HostAbortedException();
        }
    }

    public sealed class FixedTimeProvider : TimeProvider;

    public class TimeProviderOverridingFeature(TimeProvider timeProvider) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => builder.Services.AddSingleton(timeProvider);
    }

    public sealed class DisposableProbe : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    public class ThrowingOnUseFeature(DisposableProbe probe, Exception exception) : WebHostFeature
    {
        // Factory registration: the container only disposes what it creates.
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => builder.Services.AddSingleton(_ => probe);

        public override void Use(WebApplication app, WebHostFeatureContext context)
        {
            // Resolve it, so the root provider owns its disposal.
            app.Services.GetRequiredService<DisposableProbe>();

            throw exception;
        }
    }

    public class MutatingFeature(WebHostFeatureCollection features, List<string> steps) : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) => features.Add(new FirstRecordingFeature(steps));
    }

    [Fact]
    public void WhenCreate_ThenRunEveryRegisterThenEveryUseInCollectionOrder()
    {
        // arrange
        var steps = new List<string>();

        // act
        using var host = WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            features
                .Add(new FirstRecordingFeature(steps))
                .Add(new SecondRecordingFeature(steps));
        });

        // assert
        Assert.Equal(["register:first", "register:second", "use:first", "use:second"], steps);
    }

    [Fact]
    public void WhenCreate_WhenFeatureIsAddedLast_ThenItsRegistrationsOverrideBuiltinOnes()
    {
        // arrange
        var expected = new FixedTimeProvider();

        // act
        using var host = WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            // CommonFeature registers TimeProvider.System with TryAdd.
            features.Add(new TimeProviderOverridingFeature(expected));
        });

        // assert
        Assert.Same(expected, host.Services.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void WhenCreate_ThenPassAssembliesInContext()
    {
        // arrange
        var feature = new ContextCapturingFeature();
        var assemblies = ScanFreeAssemblies;

        // act
        using var host = WebHostFactory.Create(
            args: TestHostSettings.Required,
            assemblies: assemblies,
            configureFeatures: features => features.Add(feature)
        );

        // assert
        Assert.Multiple(
            () => Assert.NotNull(feature.Captured),
            () => Assert.Equal([.. LibraryAssemblies, .. assemblies], feature.Captured!.Assemblies),
            () => Assert.NotNull(feature.Captured!.Logger)
        );
    }

    [Fact]
    public void WhenCreate_WhenFeatureThrows_ThenRethrowSameException()
    {
        // arrange
        var expected = new InvalidOperationException("boom");

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            features.Add(new ThrowingFeature(expected));
        }));

        // assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public void WhenCreate_WhenFeatureUseThrows_ThenDisposeAppAndRethrowSameException()
    {
        // arrange
        var probe = new DisposableProbe();
        var expected = new InvalidOperationException("boom");

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            features.Add(new ThrowingOnUseFeature(probe, expected));
        }));

        // assert
        Assert.Multiple(
            () => Assert.Same(expected, actual),
            () => Assert.True(probe.Disposed)
        );
    }

    [Fact]
    public void WhenCreate_WhenFeatureChangesCollectionWhileRegistering_ThenUseRunsOnlyForOriginalFeatures()
    {
        // arrange
        var steps = new List<string>();

        // act
        using var host = WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            features.Add(new MutatingFeature(features, steps));
        });

        // assert
        Assert.Empty(steps);
    }

    [Fact]
    public void WhenCreate_ThenFeaturesDelegateReceivesDefaultFeatures()
    {
        // arrange
        Type[] actual = [];

        // act
        using var host = WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            actual = [.. features.Select(feature => feature.GetType())];
        });

        // assert
        Assert.Equal(
            WebHostFeatureCollection.CreateDefault().Select(feature => feature.GetType()),
            actual
        );
    }

    [Fact]
    public void WhenCreate_WhenNoArgumentsGiven_ThenBuildHost()
    {
        // act
        using var host = WebHostFactory.Create(args: TestHostSettings.Required, assemblies: []);

        // assert
        Assert.NotNull(host.Services.GetService<TimeProvider>());
    }

    [Fact]
    public void WhenCreateMinimal_ThenFeaturesDelegateReceivesMinimalFeatures()
    {
        // arrange
        Type[] actual = [];

        // act
        using var host = WebHostFactory.CreateMinimal(args: TestHostSettings.Required, assemblies: [], configureFeatures: features =>
        {
            actual = [.. features.Select(feature => feature.GetType())];
        });

        // assert
        Assert.Equal(
            WebHostFeatureCollection.CreateMinimal().Select(feature => feature.GetType()),
            actual
        );
    }

    [Fact]
    public void WhenCreateMinimal_WhenNoArgumentsGiven_ThenBuildHost()
    {
        // act
        using var host = WebHostFactory.CreateMinimal(args: TestHostSettings.Required, assemblies: []);

        // assert
        Assert.NotNull(host.Services.GetService<TimeProvider>());
    }

    [Fact]
    public void WhenCreateMinimal_ThenPassArgsAndAssembliesInContext()
    {
        // arrange
        var feature = new ContextCapturingFeature();
        var assemblies = ScanFreeAssemblies;

        // act
        using var host = WebHostFactory.CreateMinimal(
            [.. TestHostSettings.Required, "--probe=minimal"],
            assemblies,
            features => features.Add(feature)
        );

        // assert
        Assert.Multiple(
            () => Assert.Equal([.. LibraryAssemblies, .. assemblies], feature.Captured!.Assemblies),
            () => Assert.Equal("minimal", host.Services.GetRequiredService<IConfiguration>()["probe"])
        );
    }

    [Fact]
    public void WhenCreate_WhenAssembliesAreNull_ThenContextHasCommonAndCallingAssembly()
    {
        // arrange
        var feature = new AbortingContextCapturingFeature();

        // act
        Assert.Throws<HostAbortedException>(() => WebHostFactory.Create(args: TestHostSettings.Required, configureFeatures: features => features.Add(feature)));

        // assert
        Assert.Equal([.. LibraryAssemblies, typeof(WebHostFactoryTests).Assembly], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreateMinimal_WhenAssembliesAreNull_ThenContextHasCommonAndCallingAssembly()
    {
        // arrange
        var feature = new AbortingContextCapturingFeature();

        // act
        Assert.Throws<HostAbortedException>(() => WebHostFactory.CreateMinimal(args: TestHostSettings.Required, configureFeatures: features => features.Add(feature)));

        // assert
        Assert.Equal([.. LibraryAssemblies, typeof(WebHostFactoryTests).Assembly], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreate_WhenAssembliesAreEmpty_ThenContextHasOnlyCommon()
    {
        // arrange
        var feature = new ContextCapturingFeature();

        // act
        using var host = WebHostFactory.Create(args: TestHostSettings.Required, assemblies: [], configureFeatures: features => features.Add(feature));

        // assert
        Assert.Equal([.. LibraryAssemblies], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreate_WhenAssembliesContainCommon_ThenCommonIsNotDuplicated()
    {
        // arrange
        var feature = new ContextCapturingFeature();

        // act
        using var host = WebHostFactory.Create(
            args: TestHostSettings.Required,
            assemblies: [.. ScanFreeAssemblies, .. LibraryAssemblies],
            configureFeatures: features => features.Add(feature)
        );

        // assert
        Assert.Equal([.. LibraryAssemblies, .. ScanFreeAssemblies], feature.Captured!.Assemblies);
    }
}
