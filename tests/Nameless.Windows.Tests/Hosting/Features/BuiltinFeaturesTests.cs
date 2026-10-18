using Microsoft.Extensions.DependencyInjection;
using Nameless.Application;
using Nameless.Windows.Hosting.Features;

namespace Nameless.Windows.Hosting.Features;

[IntegrationTest]
public class BuiltinFeaturesTests {
    [Fact]
    public void WhenCommonFeatureIsPresent_ThenRegisterTimeProviderAndApplicationContext() {
        // act
        using var host = WinHostFactory.CreateConsole(assemblies: []);

        // assert
        Assert.Multiple(
            () => Assert.Same(TimeProvider.System, host.Services.GetRequiredService<TimeProvider>()),
            () => Assert.NotNull(host.Services.GetRequiredService<IApplicationContext>())
        );
    }

    [Fact]
    public void WhenCommonFeatureIsRemoved_ThenApplicationContextIsNotRegistered() {
        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: [],
            configureFeatures: features => features.Remove<CommonFeature>()
        );

        // assert
        Assert.Null(host.Services.GetService<IApplicationContext>());
    }

    [Fact]
    public void WhenMediatorFeatureIsConfigured_ThenConfigurationIsApplied() {
        // arrange
        var called = false;

        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: [],
            configureFeatures: features => features.Get<MediatorFeature>().Configure(_ => called = true)
        );

        // assert
        Assert.True(called);
    }

    [Fact]
    public void WhenValidationFeatureIsConfigured_ThenConfigurationIsApplied() {
        // arrange
        var called = false;

        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: [],
            configureFeatures: features => features.Get<ValidationFeature>().Configure(_ => called = true)
        );

        // assert
        Assert.True(called);
    }

    [Fact]
    public void WhenLoggingFeatureIsConfigured_ThenConfigurationIsApplied() {
        // arrange
        var called = false;

        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: [],
            configureFeatures: features => features.Get<LoggingFeature>().Configure(_ => called = true)
        );

        // assert
        Assert.True(called);
    }

    [Fact]
    public void WhenBootstrapFeatureIsConfigured_ThenConfigurationIsApplied() {
        // arrange
        var called = false;

        // act
        using var host = WinHostFactory.CreateConsole(
            assemblies: [],
            configureFeatures: features => features.Get<BootstrapFeature>().Configure(_ => called = true)
        );

        // assert
        Assert.True(called);
    }
}
