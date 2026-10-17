using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Nameless.Lucene;

public sealed class RegistrationEntity {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RegistrationAnalyzerSelector : IAnalyzerSelector {
    public AnalyzerSelectorResult GetAnalyzer(string indexName) {
        return new AnalyzerSelectorResult(null, 1);
    }
}

[UnitTest]
public class LuceneRegistrationTests {
    private static LuceneRegistration CreateSut() {
        return new LuceneRegistration().WithUseAssemblyScan(false);
    }

    [Fact]
    public void WithAnalyzerSelector_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        var returned = sut.WithAnalyzerSelector<RegistrationAnalyzerSelector>();

        // assert
        Assert.Multiple(
            () => Assert.Same(sut, returned),
            () => Assert.Contains(typeof(RegistrationAnalyzerSelector), sut.AnalyzerSelectors)
        );
    }

    [Fact]
    public void WithAnalyzerSelector_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithAnalyzerSelector(typeof(string)));
    }
    
    [Fact]
    public void RegisterLucene_RegistersProviders() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();
services.AddSingleton(new Mock<IO.IFileProvider>().Object);

        // act
        var returned = services.RegisterLucene(r => r.WithUseAssemblyScan(false).WithAnalyzerSelector<RegistrationAnalyzerSelector>());
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.Multiple(
            () => Assert.Same(services, returned),
            () => Assert.IsType<AnalyzerProvider>(provider.GetRequiredService<IAnalyzerProvider>()),
            () => Assert.Single(provider.GetServices<IAnalyzerSelector>()),
            () => Assert.NotNull(provider.GetRequiredService<IIndexProvider>())
        );
    }
}

[UnitTest]
public class LuceneExtensionsAndResponsesTests {
    [Fact]
    public void IndexExtensions_Count_UsesMatchAllQuery() {
        // arrange
        var index = new Mock<IIndex>();
        index.Setup(i => i.Count(It.IsAny<Query>())).Returns(5);

        // act
        var actual = index.Object.Count();

        // assert
        Assert.Multiple(
            () => Assert.Equal(5, actual.Value),
            () => index.Verify(i => i.Count(It.IsAny<MatchAllDocsQuery>()), Times.Once)
        );
    }

    [Fact]
    public void IndexExtensions_SearchOverloads_ForwardWithDefaults() {
        // arrange
        var index = new Mock<IIndex>();
        index.Setup(i => i.Search(It.IsAny<Query>(), It.IsAny<Sort>(), It.IsAny<int>())).Returns([]);
        var query = new MatchAllDocsQuery();
        var sort = new Sort(new SortField("x", SortFieldType.STRING));

        // act
        _ = index.Object.Search(query).ToList();
        _ = index.Object.Search(query, sort).ToList();
        _ = index.Object.Search(query, 7).ToList();

        // assert
        Assert.Multiple(
            () => index.Verify(i => i.Search(query, Sort.RELEVANCE, LuceneConstants.MaximumQueryResults), Times.Once),
            () => index.Verify(i => i.Search(query, sort, LuceneConstants.MaximumQueryResults), Times.Once),
            () => index.Verify(i => i.Search(query, Sort.RELEVANCE, 7), Times.Once)
        );
    }

    [Fact]
    public void IndexProviderExtensions_Get_UsesDefaultIndexName() {
        // arrange
        var provider = new Mock<IIndexProvider>();
        var index = new Mock<IIndex>().Object;
        provider.Setup(p => p.Get(null)).Returns(index);

        // act
        var actual = provider.Object.Get();

        // assert
        Assert.Same(index, actual);
    }
    
    [Fact]
    public void LuceneDefaults_ExposeExpectedValues() {
        // act & assert
        Assert.Multiple(
            () => Assert.False(string.IsNullOrWhiteSpace(LuceneDefaults.IndexName)),
            () => Assert.Same(Sort.RELEVANCE, LuceneDefaults.Sort),
            () => Assert.IsType<StandardAnalyzer>(LuceneDefaults.Analyzer),
            () => Assert.NotEqual(default, LuceneDefaults.Version)
        );
    }
}
