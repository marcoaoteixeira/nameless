using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nameless.Web.Hosting.Features;

[IntegrationTest]
public class HealthCheckFeatureTests
{
    [Fact]
    public async Task WhenUse_ThenLivenessEndpointIsAvailable()
    {
        // arrange
        var sut = new HealthCheckFeature();
        var context = new WebHostFeatureContext([], NullLogger.Instance);
        var builder = WebApplication.CreateBuilder(TestHostSettings.Required);

        builder.WebHost.UseTestServer();

        sut.Register(builder, context);

        await using var app = builder.Build();

        sut.Use(app, context);

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        // act
        var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
