using Microsoft.Extensions.DependencyInjection;
using Nameless.Collections.Generic;
using Nameless.Compression;
using Nameless.ProducerConsumer.RabbitMQ.Options;

namespace Nameless.Configuration;

[UnitTest]
public class ConfigurationExceptionsTests {
    [Fact]
    public void MissingConfigurationException_WithSection_MentionsSection() {
        // act
        var sut = new MissingConfigurationException("MySection");

        // assert
        Assert.Contains("MySection", sut.Message);
    }

    [Fact]
    public void MissingConfigurationException_WithSectionAndKey_MentionsBoth() {
        // act
        var sut = new MissingConfigurationException("MySection", "MyKey");

        // assert
        Assert.Multiple(
            () => Assert.Contains("MySection", sut.Message),
            () => Assert.Contains("MyKey", sut.Message)
        );
    }

    [Fact]
    public void MissingConfigurationException_WithMessageAndInner_KeepsBoth() {
        // arrange
        var inner = new InvalidOperationException("inner");

        // act
        var sut = new MissingConfigurationException("custom", inner);

        // assert
        Assert.Multiple(
            () => Assert.Equal("custom", sut.Message),
            () => Assert.Same(inner, sut.InnerException)
        );
    }

    [Fact]
    public void ConfigurationBindingException_MentionsPathAndType() {
        // act
        var sut = new ConfigurationBindingException("Some:Path", typeof(string));

        // assert
        Assert.Multiple(
            () => Assert.Contains("Some:Path", sut.Message),
            () => Assert.Contains("System.String", sut.Message)
        );
    }

    [Fact]
    public void MissingEntityIDException_MentionsEntityType() {
        // act
        var sut = new MissingEntityIDException(typeof(List<int>));

        // assert
        Assert.Contains("List", sut.Message);
    }

    [Fact]
    public void ServerOptions_UseCredentials_RequiresUsernameAndPassword() {
        // act & assert
        Assert.Multiple(
            () => Assert.True(new ServerOptions { Username = "u", Password = "p" }.UseCredentials),
            () => Assert.False(new ServerOptions { Username = "u" }.UseCredentials),
            () => Assert.False(new ServerOptions { Password = "p" }.UseCredentials)
        );
    }

    [Fact]
    public void RegisterZipCompressor_RegistersSingletonCompressor() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // act
        var returned = services.RegisterZipCompressor();
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.Multiple(
            () => Assert.Same(services, returned),
            () => Assert.IsType<ZipCompressor>(provider.GetRequiredService<ICompressor>()),
            () => Assert.Same(provider.GetRequiredService<ICompressor>(), provider.GetRequiredService<ICompressor>())
        );
    }

    [Fact]
    public void ToPage_ReturnsRequestedWindowAndTotals() {
        // arrange
        var source = Enumerable.Range(1, 25).AsQueryable();

        // act
        var page = source.ToPage(start: 10, limit: 10);

        // assert
        Assert.Multiple(
            () => Assert.Equal([11, 12, 13, 14, 15, 16, 17, 18, 19, 20], page),
            () => Assert.Equal(25, page.TotalCount),
            () => Assert.Equal(2, page.Number),
            () => Assert.Equal(3, page.PageCount),
            () => Assert.True(page.HasPrevious),
            () => Assert.True(page.HasNext)
        );
    }

    [Fact]
    public void ToPage_WithDefaults_ReturnsFirstPage() {
        // arrange
        var source = Enumerable.Range(1, 5).AsQueryable();

        // act
        var page = source.ToPage();

        // assert
        Assert.Multiple(
            () => Assert.Equal(5, page.Count()),
            () => Assert.False(page.HasNext),
            () => Assert.False(page.HasPrevious)
        );
    }
}
