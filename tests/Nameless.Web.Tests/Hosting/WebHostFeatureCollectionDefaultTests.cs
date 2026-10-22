using Nameless.Web.Hosting.Features;

namespace Nameless.Web.Hosting;

[UnitTest]
public class WebHostFeatureCollectionDefaultTests
{
    [Fact]
    public void WhenCreateDefault_ThenContainBuiltinFeaturesInPipelineOrder()
    {
        // arrange
        Type[] expected =
        [
            typeof(CommonFeature),
            typeof(ExceptionHandlingFeature),
            typeof(LoggingFeature),
            typeof(HttpRequestCorrelationFeature),
            typeof(EndpointRoutingFeature),
            typeof(HealthCheckFeature),
            typeof(HttpsRedirectionFeature),
            typeof(CorsFeature),
            typeof(RateLimiterFeature),
            typeof(OutputCacheFeature),
            typeof(RequestTimeoutFeature),
            typeof(AuthFeature),
            typeof(AntiforgeryFeature),
            typeof(OpenApiFeature),
            typeof(ScalarFeature),
            typeof(DataProtectionFeature),
            typeof(MediatorFeature),
            typeof(OpenTelemetryFeature),
            typeof(PeriodicWorkersFeature),
            typeof(ResilienceFeature),
            typeof(ServiceDiscoveryFeature),
            typeof(StatusReportingFeature),
            typeof(ValidationFeature)
        ];

        // act
        var actual = WebHostFeatureCollection.CreateDefault();

        // assert
        Assert.Equal(expected, actual.Select(feature => feature.GetType()));
    }

    [Fact]
    public void WhenCreateDefault_ThenReturnNewCollectionEveryTime()
    {
        // act
        var first = WebHostFeatureCollection.CreateDefault();
        var second = WebHostFeatureCollection.CreateDefault();

        // assert
        Assert.NotSame(first, second);
    }

    [Fact]
    public void WhenCreateMinimal_ThenContainMinimalBuiltinFeaturesInPipelineOrder()
    {
        // arrange
        Type[] expected =
        [
            typeof(CommonFeature),
            typeof(ExceptionHandlingFeature),
            typeof(LoggingFeature),
            typeof(HttpRequestCorrelationFeature),
            typeof(EndpointRoutingFeature),
            typeof(HealthCheckFeature),
            typeof(CorsFeature),
            typeof(AuthFeature),
            typeof(OpenApiFeature),
            typeof(ScalarFeature),
            typeof(MediatorFeature),
            typeof(OpenTelemetryFeature),
            typeof(ResilienceFeature),
            typeof(ServiceDiscoveryFeature),
            typeof(ValidationFeature)
        ];

        // act
        var actual = WebHostFeatureCollection.CreateMinimal();

        // assert
        Assert.Equal(expected, actual.Select(feature => feature.GetType()));
    }

    [Fact]
    public void WhenCreateMinimal_ThenReturnNewCollectionEveryTime()
    {
        // act
        var first = WebHostFeatureCollection.CreateMinimal();
        var second = WebHostFeatureCollection.CreateMinimal();

        // assert
        Assert.NotSame(first, second);
    }
}
