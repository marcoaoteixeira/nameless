using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Wpf.Ui;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting.Features;

[IntegrationTest]
public class FeatureRegistrationTests {
    // Navigation needs an application INavigationWindow; it has its own test.
    public static TheoryData<Type> FeatureTypes => [
        .. WinHostFeatureCollection
            .CreateWinAppBase()
            .Select(feature => feature.GetType())
            .Where(type => type != typeof(NavigationFeature))
    ];

    [Theory]
    [MemberData(nameof(FeatureTypes))]
    public void WhenRegister_ThenAddServicesToCollection(Type featureType) {
        // arrange
        var sut = (WinHostFeature)Activator.CreateInstance(featureType)!;
        var context = new WinHostFeatureContext(
            [typeof(AssemblyMarker).Assembly, typeof(AssemblyMarkerWindows).Assembly],
            NullLogger.Instance
        );
        var builder = new WinHostBuilder(Host.CreateDefaultBuilder());
        var before = -1;
        var after = -1;

        builder.ConfigureServices((_, services) => before = services.Count);

        // act
        sut.Register(builder, context);

        builder.ConfigureServices((_, services) => after = services.Count);

        using var host = builder.Build();

        // assert
        Assert.True(after > before, $"{sut.Name} did not register any service.");
    }

    [Fact]
    public void WhenRegisterNavigation_ThenRegisterTheNavigationWindow() {
        // arrange
        // A proxy type stands in for the application window, which would
        // need a real WPF Window instance.
        var windowType = Mock.Of<INavigationWindow>().GetType();
        var sut = new NavigationFeature();
        var context = new WinHostFeatureContext([], NullLogger.Instance);
        var builder = new WinHostBuilder(Host.CreateDefaultBuilder());

        sut.Configure(registration => registration.RegisterNavigationWindow(windowType));

        IServiceCollection? registered = null;

        // act
        sut.Register(builder, context);

        builder.ConfigureServices((_, services) => registered = services);

        using var host = builder.Build();

        // assert
        Assert.Contains(
            registered!,
            descriptor => descriptor.ServiceType == typeof(INavigationWindow)
                       && descriptor.ImplementationType == windowType
        );
    }
}
