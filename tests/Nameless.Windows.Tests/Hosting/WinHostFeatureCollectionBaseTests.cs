using Nameless.Windows.Hosting.Features;

namespace Nameless.Windows.Hosting;

[UnitTest]
public class WinHostFeatureCollectionBaseTests {
    [Fact]
    public void WhenCreateWinAppBase_ThenContainEveryBuiltinFeature() {
        // arrange
        Type[] expected = [
            typeof(CommonFeature),
            typeof(AppConfigurationManagerFeature),
            typeof(BootstrapFeature),
            typeof(CompressorFeature),
            typeof(ContentDialogServiceFeature),
            typeof(DocumentServicesFeature),
            typeof(FileProviderFeature),
            typeof(FileSystemDialogFeature),
            typeof(GitHubHttpClientFeature),
            typeof(HttpClientDefaultsFeature),
            typeof(LocalizationFeature),
            typeof(LoggingFeature),
            typeof(LuceneFeature),
            typeof(MediatorFeature),
            typeof(MessageDialogFeature),
            typeof(MessengerFeature),
            typeof(NavigationFeature),
            typeof(OfficeFeature),
            typeof(ResilienceFeature),
            typeof(SnackBarFeature),
            typeof(StatusReportingFeature),
            typeof(TaskRunnerFeature),
            typeof(ValidationFeature),
            typeof(ViewModelsFeature),
            typeof(WindowFactoryFeature)
        ];

        // act
        var actual = WinHostFeatureCollection.CreateWinAppBase();

        // assert
        Assert.Equal(expected, actual.Select(feature => feature.GetType()));
    }

    [Fact]
    public void WhenCreateWinAppBase_ThenReturnNewCollectionEveryTime() {
        // act
        var first = WinHostFeatureCollection.CreateWinAppBase();
        var second = WinHostFeatureCollection.CreateWinAppBase();

        // assert
        Assert.NotSame(first, second);
    }

    [Fact]
    public void WhenCreateConsole_ThenContainConsoleFeatures() {
        // arrange
        Type[] expected = [
            typeof(CommonFeature),
            typeof(AppConfigurationManagerFeature),
            typeof(BootstrapFeature),
            typeof(CompressorFeature),
            typeof(FileProviderFeature),
            typeof(GitHubHttpClientFeature),
            typeof(HttpClientDefaultsFeature),
            typeof(LocalizationFeature),
            typeof(LoggingFeature),
            typeof(LuceneFeature),
            typeof(MediatorFeature),
            typeof(ResilienceFeature),
            typeof(ValidationFeature)
        ];

        // act
        var actual = WinHostFeatureCollection.CreateConsoleBase();

        // assert
        Assert.Equal(expected, actual.Select(feature => feature.GetType()));
    }

    [Fact]
    public void WhenCreateConsole_ThenReturnNewCollectionEveryTime() {
        // act
        var first = WinHostFeatureCollection.CreateConsoleBase();
        var second = WinHostFeatureCollection.CreateConsoleBase();

        // assert
        Assert.NotSame(first, second);
    }

    [Fact]
    public void WhenCreateWinAppBase_ThenEveryFeatureHasDistinctName() {
        // act
        var actual = WinHostFeatureCollection.CreateWinAppBase().Select(feature => feature.Name).ToArray();

        // assert
        Assert.Equal(actual.Length, actual.Distinct().Count());
    }
}
