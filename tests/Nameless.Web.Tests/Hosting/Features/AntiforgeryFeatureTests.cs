using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Nameless.Web.Hosting.Features;

[UnitTest]
public class AntiforgeryFeatureTests
{
    private static AntiforgeryOptions RegisterAndResolve(AntiforgeryFeature feature)
    {
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());

        feature.Register(builder, new WebHostFeatureContext([], NullLogger.Instance));

        using var provider = builder.Services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;
    }

    [Fact]
    public void WhenRegister_WhenNotConfigured_ThenApplyDefaults()
    {
        // arrange
        var sut = new AntiforgeryFeature();

        // act
        var actual = RegisterAndResolve(sut);

        // assert
        Assert.Multiple(
            () => Assert.Equal("X-CSRF-TOKEN", actual.HeaderName),
            () => Assert.Equal("X-CSRF-TOKEN", actual.Cookie.Name),
            () => Assert.True(actual.Cookie.HttpOnly),
            () => Assert.Equal(CookieSecurePolicy.Always, actual.Cookie.SecurePolicy)
        );
    }

    [Fact]
    public void WhenConfigure_WhenCalledAfterRegister_ThenHaveNoEffect()
    {
        // arrange
        var sut = new AntiforgeryFeature();
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions());

        sut.Register(builder, new WebHostFeatureContext([], NullLogger.Instance));
        sut.Configure(opts => opts.HeaderName = "X-LATE");

        using var provider = builder.Services.BuildServiceProvider();

        // act
        var actual = provider.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;

        // assert
        Assert.Equal("X-CSRF-TOKEN", actual.HeaderName);
    }

    [Fact]
    public void WhenRegister_WhenConfigured_ThenExtendDefaults()
    {
        // arrange
        var sut = new AntiforgeryFeature();

        sut.Configure(opts => opts.HeaderName = "X-CUSTOM");

        // act
        var actual = RegisterAndResolve(sut);

        // assert
        Assert.Multiple(
            () => Assert.Equal("X-CUSTOM", actual.HeaderName),
            () => Assert.Equal("X-CSRF-TOKEN", actual.Cookie.Name),
            () => Assert.Equal(CookieSecurePolicy.Always, actual.Cookie.SecurePolicy)
        );
    }
}
