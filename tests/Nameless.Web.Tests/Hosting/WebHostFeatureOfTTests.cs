
namespace Nameless.Web.Hosting;

[UnitTest]
public class WebHostFeatureOfTTests
{
    public class Registration
    {
        public List<string> Steps { get; } = [];
    }

    public class ConfigurableFeature : WebHostFeature<Registration>
    {
        public Action<Registration>? Exposed => ConfigureAction;
    }

    [Fact]
    public void WhenConfigureIsNeverCalled_ThenConfigureRegistrationIsNull()
    {
        // act
        var sut = new ConfigurableFeature();

        // assert
        Assert.Null(sut.Exposed);
    }

    [Fact]
    public void WhenConfigure_WhenCalledManyTimes_ThenComposeInCallOrder()
    {
        // arrange
        var sut = new ConfigurableFeature();
        var registration = new Registration();

        // act
        sut.Configure(r => r.Steps.Add("first"))
           .Configure(r => r.Steps.Add("second"));

        sut.Exposed!(registration);

        // assert
        Assert.Equal(["first", "second"], registration.Steps);
    }

    [Fact]
    public void WhenConfigure_ThenReturnSameFeature()
    {
        // arrange
        var sut = new ConfigurableFeature();

        // act
        var actual = sut.Configure(_ => { });

        // assert
        Assert.Same(sut, actual);
    }

    [Fact]
    public void WhenConfigure_WhenDelegateIsNull_ThenThrowArgumentNullException()
    {
        // arrange
        var sut = new ConfigurableFeature();

        // act & assert
        Assert.Throws<ArgumentNullException>(() => sut.Configure(null!));
    }
}
