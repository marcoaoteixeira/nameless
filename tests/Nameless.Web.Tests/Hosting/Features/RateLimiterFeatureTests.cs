using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nameless.Web.Hosting.Features;

[IntegrationTest]
public class RateLimiterFeatureTests
{
    [Fact]
    public async Task WhenUse_ThenRequestsWithoutPolicyAreServed()
    {
        // arrange
        var sut = new RateLimiterFeature();
        var context = new WebHostFeatureContext([], NullLogger.Instance);
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());

        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();

        sut.Register(builder, context);

        await using var app = builder.Build();

        app.UseRouting();
        sut.Use(app, context);
        app.MapGet("/", () => "ok");

        await app.StartAsync(TestContext.Current.CancellationToken);

        var client = app.GetTestClient();

        // act
        var response = await client.GetAsync("/", TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
