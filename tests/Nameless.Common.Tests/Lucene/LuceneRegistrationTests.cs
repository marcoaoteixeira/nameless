using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nameless.ObjectModel;

namespace Nameless.Lucene;

public sealed class RegistrationEntity {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class RegistrationEntityMapping : IEntityMapping<RegistrationEntity> {
    public void Map(IEntityDescriptor<RegistrationEntity> descriptor) {
        descriptor.SetID(e => e.Id);
        descriptor.SetProperty(e => e.Name, PropertyOptions.Store);
    }
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
    public void WithMapping_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithMapping<RegistrationEntityMapping, RegistrationEntity>();

        // assert
        Assert.Contains(typeof(RegistrationEntityMapping), sut.Mappings);
    }

    [Fact]
    public void WithMapping_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithMapping(typeof(string)));
    }

    [Fact]
    public void WithUseRepository_SetsFlag() {
        // act
        var sut = CreateSut().WithUseRepository(true);

        // assert
        Assert.True(sut.UseRepository);
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

    [Fact]
    public void RegisterLucene_WithRepository_RegistersRepositoryServices() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new Mock<IO.IFileProvider>().Object);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Lucene:UseInMemory"] = "true" })
            .Build();

        // act
        services.RegisterLucene(r => r
            .WithUseAssemblyScan(false)
            .WithUseRepository(true)
            .WithMapping<RegistrationEntityMapping, RegistrationEntity>(), configuration);
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.Multiple(
            () => Assert.NotNull(provider.GetRequiredService<IRepository>()),
            () => Assert.NotNull(provider.GetRequiredService<IMapper>()),
            () => Assert.NotNull(provider.GetRequiredService<IEntityDescriptorProvider>()),
            () => Assert.IsType<RegistrationEntityMapping>(provider.GetRequiredService<IEntityMapping<RegistrationEntity>>())
        );
    }

    [Fact]
    public void RegisterLucene_WithoutRepository_DoesNotRegisterRepository() {
        // arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // act
        services.RegisterLucene(r => r.WithUseAssemblyScan(false));

        // assert
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRepository));
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
    public async Task Responses_FromCountAndErrors_BuildSuccessAndFailure() {
        // act
        var insertOk = await InsertEntitiesResponse.From(3);
        var insertKo = await InsertEntitiesResponse.From(Error.Failure("x"));
        var updateOk = await UpdateEntitiesResponse.From(2);
        var updateKo = await UpdateEntitiesResponse.From(Error.Failure("x"), Error.Failure("y"));
        var deleteOk = await DeleteEntitiesResponse.From(1);
        var deleteKo = await DeleteEntitiesResponse.From(Error.Failure("x"));
        var byQueryOk = await DeleteEntitiesByQueryResponse.From(4);
        var byQueryKo = await DeleteEntitiesByQueryResponse.From(Error.Failure("x"));

        // assert
        Assert.Multiple(
            () => Assert.Equal(3, insertOk.Value.Count),
            () => Assert.False(insertKo.Success),
            () => Assert.Equal(2, updateOk.Value.Count),
            () => Assert.Equal(2, updateKo.Errors.Length),
            () => Assert.Equal(1, deleteOk.Value.Count),
            () => Assert.False(deleteKo.Success),
            () => Assert.Equal(4, byQueryOk.Value.Count),
            () => Assert.False(byQueryKo.Success)
        );
    }

    [Fact]
    public void Responses_ImplicitOperators_BuildFromMetadataErrorAndErrors() {
        // act
        InsertEntitiesResponse a = new InsertEntitiesMetadata(1);
        InsertEntitiesResponse b = Error.Failure("x");
        UpdateEntitiesResponse c = new UpdateDocumentsMetadata(1);
        UpdateEntitiesResponse d = Error.Failure("x");
        DeleteEntitiesResponse e = new DeleteDocumentsMetadata(1);
        DeleteEntitiesResponse f = Error.Failure("x");
        DeleteEntitiesByQueryResponse g = new DeleteDocumentsByQueryMetadata(1);
        DeleteEntitiesByQueryResponse h = new[] { Error.Failure("x") };

        // assert
        Assert.Multiple(
            () => Assert.True(a.Success),
            () => Assert.False(b.Success),
            () => Assert.True(c.Success),
            () => Assert.False(d.Success),
            () => Assert.True(e.Success),
            () => Assert.False(f.Success),
            () => Assert.True(g.Success),
            () => Assert.False(h.Success)
        );
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
