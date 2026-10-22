using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nameless.Web.Hosting;

[UnitTest]
public class WebHostFeatureContextTests
{
    [Fact]
    public void WhenCreated_ThenExposeAssembliesAndLogger()
    {
        // arrange
        var assemblies = new[] { typeof(WebHostFeatureContextTests).Assembly };
        var logger = NullLogger.Instance;

        // act
        var sut = new WebHostFeatureContext(assemblies, logger);

        // assert
        Assert.Multiple(
            () => Assert.Equal(assemblies, sut.Assemblies),
            () => Assert.Same(logger, sut.Logger)
        );
    }

    [Fact]
    public void WhenCreated_WhenSourceAssembliesChange_ThenContextIsNotAffected()
    {
        // arrange
        var assemblies = new[] { typeof(WebHostFeatureContextTests).Assembly };
        var sut = new WebHostFeatureContext(assemblies, NullLogger.Instance);

        // act
        assemblies[0] = typeof(object).Assembly;

        // assert
        Assert.Same(typeof(WebHostFeatureContextTests).Assembly, Assert.Single(sut.Assemblies));
    }

    [Fact]
    public void WhenCreated_WhenAssembliesIsNull_ThenThrowArgumentNullException()
    {
        // act & assert
        Assert.Throws<ArgumentNullException>(() => new WebHostFeatureContext(null!, NullLogger.Instance));
    }

    [Fact]
    public void WhenCreated_WhenLoggerIsNull_ThenThrowArgumentNullException()
    {
        // act & assert
        Assert.Throws<ArgumentNullException>(() => new WebHostFeatureContext([], (ILogger)null!));
    }
}
