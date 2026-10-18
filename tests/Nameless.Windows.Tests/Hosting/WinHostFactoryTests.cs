using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Nameless.Windows.Hosting.Features;
using Nameless.Windows.Hosting.Wrappers;
using Wpf.Ui;

namespace Nameless.Windows.Hosting;

[IntegrationTest]
public class WinHostFactoryTests {
    private static readonly Assembly[] TestAssemblies = [typeof(Assert).Assembly];
    private static readonly Assembly CommonAssembly = typeof(AssemblyMarker).Assembly;
    private static readonly Assembly WindowsAssembly = typeof(AssemblyMarkerWindows).Assembly;

    public class FirstRecordingFeature(List<string> steps) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) => steps.Add("register:first");
    }

    public class SecondRecordingFeature(List<string> steps) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) => steps.Add("register:second");
    }

    public class ContextCapturingFeature : WinHostFeature {
        public WinHostFeatureContext? Captured { get; private set; }

        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) => Captured = context;
    }

    public class ThrowingFeature(Exception exception) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) => throw exception;
    }

    // Aborts before the host is built, so the assemblies in the context
    // are captured without being validated by the container.
    public class AbortingContextCapturingFeature : WinHostFeature {
        public WinHostFeatureContext? Captured { get; private set; }

        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) {
            Captured = context;

            throw new HostAbortedException();
        }
    }

    public sealed class FixedTimeProvider : TimeProvider;

    public class TimeProviderOverridingFeature(TimeProvider timeProvider) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context)
            => builder.ConfigureServices((_, services) => services.AddSingleton(timeProvider));
    }

    public class MutatingFeature(WinHostFeatureCollection features, List<string> steps) : WinHostFeature {
        public override void Register(WinHostBuilder builder, WinHostFeatureContext context) => features.Add(new FirstRecordingFeature(steps));
    }

    [Fact]
    public void WhenCreateConsole_ThenRunEveryRegisterInCollectionOrder() {
        // arrange
        var steps = new List<string>();

        // act
        using var host = WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            features.Add(new FirstRecordingFeature(steps))
                    .Add(new SecondRecordingFeature(steps));
        });

        // assert
        Assert.Equal(["register:first", "register:second"], steps);
    }

    [Fact]
    public void WhenCreateConsole_WhenFeatureIsAddedLast_ThenItsRegistrationsOverrideBuiltinOnes() {
        // arrange
        var expected = new FixedTimeProvider();

        // act
        using var host = WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            // CommonFeature registers TimeProvider.System with TryAdd.
            features.Add(new TimeProviderOverridingFeature(expected));
        });

        // assert
        Assert.Same(expected, host.Services.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void WhenCreateConsole_ThenPassAssembliesInContext() {
        // arrange
        var feature = new ContextCapturingFeature();

        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: TestAssemblies,
            configureFeatures: features => features.Add(feature)
        );

        // assert
        Assert.Multiple(
            () => Assert.NotNull(feature.Captured),
            () => Assert.Equal([CommonAssembly, WindowsAssembly, .. TestAssemblies], feature.Captured!.Assemblies),
            () => Assert.NotNull(feature.Captured!.Logger)
        );
    }

    [Fact]
    public void WhenCreateConsole_WhenFeatureThrows_ThenRethrowSameException() {
        // arrange
        var expected = new InvalidOperationException("boom");

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            features.Add(new ThrowingFeature(expected));
        }));

        // assert
        Assert.Same(expected, actual);
    }

    [Fact]
    public void WhenCreateConsole_WhenFeatureChangesCollectionWhileRegistering_ThenOnlyOriginalFeaturesRegister() {
        // arrange
        var steps = new List<string>();

        // act
        using var host = WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            features.Add(new MutatingFeature(features, steps));
        });

        // assert
        Assert.Empty(steps);
    }

    [Fact]
    public void WhenCreateWinApp_ThenFeaturesDelegateReceivesWinAppFeatures() {
        // arrange
        Type[] actual = [];

        // act
        // Default features need application-supplied services to build.
        Assert.Throws<HostAbortedException>(() => WinHostFactory.CreateWinApp(assemblies: [], configureFeatures: features => {
            actual = [.. features.Select(feature => feature.GetType())];
            features.Add(new ThrowingFeature(new HostAbortedException()));
        }));

        // assert
        Assert.Equal(
            WinHostFeatureCollection.CreateWinAppBase().Select(feature => feature.GetType()),
            actual
        );
    }

    [Fact]
    public void WhenCreateConsole_ThenFeaturesDelegateReceivesConsoleFeatures() {
        // arrange
        Type[] actual = [];

        // act
        using var host = WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => {
            actual = [.. features.Select(feature => feature.GetType())];
        });

        // assert
        Assert.Equal(
            WinHostFeatureCollection.CreateConsoleBase().Select(feature => feature.GetType()),
            actual
        );
    }

    [Fact]
    public void WhenCreateConsole_WhenNoArgumentsGiven_ThenBuildHost() {
        // act
        using var host = WinHostFactory.CreateConsole(assemblies: []);

        // assert
        Assert.NotNull(host.Services.GetService<TimeProvider>());
    }

    [Fact]
    public void WhenCreateConsole_ThenPassArgsAndAssembliesInContext() {
        // arrange
        var feature = new ContextCapturingFeature();

        // act
        using var host = WinHostFactory.CreateConsole(
            ["--probe=minimal"],
            TestAssemblies,
            configureFeatures: features => features.Add(feature)
        );

        // assert
        Assert.Multiple(
            () => Assert.Equal([CommonAssembly, WindowsAssembly, .. TestAssemblies], feature.Captured!.Assemblies),
            () => Assert.Equal("minimal", host.Services.GetRequiredService<IConfiguration>()["probe"])
        );
    }

    [Fact]
    public void WhenCreateWinApp_WhenAssembliesAreNull_ThenContextHasLibrariesAndCallingAssembly() {
        // arrange
        var feature = new AbortingContextCapturingFeature();

        // act
        Assert.Throws<HostAbortedException>(() => WinHostFactory.CreateWinApp(configureFeatures: features => features.Add(feature)));

        // assert
        Assert.Equal([CommonAssembly, WindowsAssembly, typeof(WinHostFactoryTests).Assembly], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreateConsole_WhenAssembliesAreNull_ThenContextHasLibrariesAndCallingAssembly() {
        // arrange
        var feature = new AbortingContextCapturingFeature();

        // act
        Assert.Throws<HostAbortedException>(() => WinHostFactory.CreateConsole(configureFeatures: features => features.Add(feature)));

        // assert
        Assert.Equal([CommonAssembly, WindowsAssembly, typeof(WinHostFactoryTests).Assembly], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreateConsole_WhenAssembliesAreEmpty_ThenContextHasOnlyLibraries() {
        // arrange
        var feature = new ContextCapturingFeature();

        // act
        using var host = WinHostFactory.CreateConsole(assemblies: [], configureFeatures: features => features.Add(feature));

        // assert
        Assert.Equal([CommonAssembly, WindowsAssembly], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreateConsole_WhenAssembliesContainLibraries_ThenLibrariesAreNotDuplicated() {
        // arrange
        var feature = new ContextCapturingFeature();

        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: [.. TestAssemblies, WindowsAssembly, CommonAssembly],
            configureFeatures: features => features.Add(feature)
        );

        // assert
        Assert.Equal([CommonAssembly, WindowsAssembly, .. TestAssemblies], feature.Captured!.Assemblies);
    }

    [Fact]
    public void WhenCreateWinApp_WhenNavigationWindowIsProvided_ThenBuildHostWithEveryFeature() {
        // arrange
        // A proxy type stands in for the application window, which would
        // need a real WPF Window instance.
        var windowType = Mock.Of<INavigationWindow>().GetType();

        // act
        using var host = WinHostFactory.CreateWinApp(assemblies: [], configureFeatures: features => {
            features.Get<NavigationFeature>().Configure(
                registration => registration.RegisterNavigationWindow(windowType)
            );
        });

        // assert
        Assert.NotNull(host.Services.GetService<TimeProvider>());
    }
}
