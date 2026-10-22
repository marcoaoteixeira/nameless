using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nameless.Windows.Hosting;

[UnitTest]
public class WinHostFeatureContextTests
{
    [Fact]
    public void WhenCreated_ThenExposeAssembliesAndLogger()
    {
        // arrange
        var assemblies = new[] { typeof(WinHostFeatureContextTests).Assembly };
        var logger = NullLogger.Instance;

        // act
        var sut = new WinHostFeatureContext(assemblies, logger);

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
        var assemblies = new[] { typeof(WinHostFeatureContextTests).Assembly };
        var sut = new WinHostFeatureContext(assemblies, NullLogger.Instance);

        // act
        assemblies[0] = typeof(object).Assembly;

        // assert
        Assert.Same(typeof(WinHostFeatureContextTests).Assembly, Assert.Single(sut.Assemblies));
    }

    [Fact]
    public void WhenCreated_WhenAssembliesIsNull_ThenThrowArgumentNullException()
    {
        // act & assert
        Assert.Throws<ArgumentNullException>(() => new WinHostFeatureContext(null!, NullLogger.Instance));
    }

    [Fact]
    public void WhenCreated_WhenLoggerIsNull_ThenThrowArgumentNullException()
    {
        // act & assert
        Assert.Throws<ArgumentNullException>(() => new WinHostFeatureContext([], (ILogger)null!));
    }
}
