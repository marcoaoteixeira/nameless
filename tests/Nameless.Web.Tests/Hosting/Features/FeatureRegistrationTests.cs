using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nameless.Web.Hosting.Features;

[IntegrationTest]
public class FeatureRegistrationTests
{
    // These features add nothing to the service collection by default: they
    // only add middlewares or endpoints (the Use phase), which the factory
    // tests exercise by building the full host, or register just what is
    // explicitly configured.
    private static readonly Type[] NoDefaultServicesFeatures =
    [
        typeof(EndpointRoutingFeature),
        typeof(HttpsRedirectionFeature),
        typeof(ScalarFeature),
        typeof(ExceptionHandlingFeature),
        typeof(PeriodicWorkersFeature)
    ];

    public static TheoryData<Type> FeatureTypes => [
        .. WebHostFeatureCollection
            .CreateDefault()
            .Select(feature => feature.GetType())
            .Where(type => !NoDefaultServicesFeatures.Contains(type))
    ];

    [Theory]
    [MemberData(nameof(FeatureTypes))]
    public void WhenRegister_ThenAddServicesToCollection(Type featureType)
    {
        // arrange
        var sut = (WebHostFeature)Activator.CreateInstance(featureType)!;
        var context = new WebHostFeatureContext(
            [typeof(Nameless.AssemblyMarker).Assembly, typeof(AssemblyMarker).Assembly],
            NullLogger.Instance
        );
        var builder = WebApplication.CreateBuilder(TestHostSettings.Required);
        var before = builder.Services.Count;

        // act
        sut.Register(builder, context);

        // assert
        Assert.True(builder.Services.Count > before, $"{sut.Name} did not register any service.");
    }
}
