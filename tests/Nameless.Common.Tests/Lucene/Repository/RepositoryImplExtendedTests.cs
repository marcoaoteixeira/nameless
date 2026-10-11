using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Nameless.IO;
using Nameless.Testing.Tools.Mockers.Logging;

namespace Nameless.Lucene.Repository;

[IntegrationTest]
public class RepositoryImplExtendedTests : IDisposable {
    private readonly string _tempDir;
    private readonly Index _index;
    private readonly RepositoryImpl _sut;

    private class RepoEntity {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    private class RepoEntityMapping : IEntityMapping<RepoEntity> {
        public void Map(IEntityDescriptor<RepoEntity> descriptor) {
            descriptor.SetID(e => e.Id);
            descriptor.SetProperty(e => e.Title, PropertyOptions.Store);
        }
    }

    public RepositoryImplExtendedTests() {
        _tempDir = SysPath.Combine(SysPath.GetTempPath(), $"lucene-repo-ext-{Guid.NewGuid():N}");
        SysDirectory.CreateDirectory(_tempDir);

        _index = CreateIndex(_tempDir);

        var analyzerProvider = new Mock<IAnalyzerProvider>();
        analyzerProvider
            .Setup(ap => ap.GetAnalyzer(It.IsAny<string>()))
            .Returns(LuceneDefaults.Analyzer);

        var indexProvider = new Mock<IIndexProvider>();
        indexProvider
            .Setup(ip => ip.Get(It.IsAny<string>()))
            .Returns(_index);

        var services = new ServiceCollection();
        services.AddSingleton<IEntityMapping<RepoEntity>, RepoEntityMapping>();
        var serviceProvider = services.BuildServiceProvider();

        var mapper = new Mapper(new EntityDescriptorProvider(serviceProvider));

        _sut = new RepositoryImpl(analyzerProvider.Object, indexProvider.Object, mapper);
    }

    public void Dispose() {
        _index.Dispose();

        if (SysDirectory.Exists(_tempDir)) {
            SysDirectory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteAsync_WithMatchingEntities_RemovesFromIndex() {
        // Arrange
        var entity = new RepoEntity { Id = "delA", Title = "Delete Me" };
        await _sut.InsertAsync(new InsertEntitiesRequest<RepoEntity>([entity]), TestContext.Current.CancellationToken);

        var deleteRequest = new DeleteEntitiesRequest<RepoEntity>([entity]);

        // Act
        var response = await _sut.DeleteAsync(deleteRequest, TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "delA", useWildcard: false).Mandatory().ExactMatch()
        };
        var found = await _sut.SearchAsync<RepoEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert — response.Value.Count reports the number of matched documents before deletion
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Equal(1, response.Value.Count),
            () => Assert.Empty(found)
        );
    }

    [Fact]
    public async Task DeleteByQueryAsync_WithMatchingQuery_RemovesFromIndex() {
        // Arrange
        var entity = new RepoEntity { Id = "delqA", Title = "Delete By Query" };
        await _sut.InsertAsync(new InsertEntitiesRequest<RepoEntity>([entity]), TestContext.Current.CancellationToken);

        var deleteByQueryRequest = new DeleteEntitiesByQueryRequest {
            Query = qb => qb.WithField("Id", "delqA", useWildcard: false).Mandatory().ExactMatch()
        };

        // Act
        var response = await _sut.DeleteByQueryAsync(deleteByQueryRequest, TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "delqA", useWildcard: false).Mandatory().ExactMatch()
        };
        var found = await _sut.SearchAsync<RepoEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Empty(found)
        );
    }

    [Fact]
    public async Task UpdateAsync_WithModifiedEntities_UpdatesValues() {
        // Arrange — IDs must be all-lowercase because the StandardAnalyzer lowercases query terms
        var original = new RepoEntity { Id = "upd1", Title = "Old Title" };
        await _sut.InsertAsync(new InsertEntitiesRequest<RepoEntity>([original]), TestContext.Current.CancellationToken);

        var updated = new RepoEntity { Id = "upd1", Title = "New Title" };

        // Act
        var response = await _sut.UpdateAsync(new UpdateEntitiesRequest<RepoEntity>([updated]), TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "upd1", useWildcard: false).Mandatory().ExactMatch()
        };
        var found = await _sut.SearchAsync<RepoEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Single(found),
            () => Assert.Equal("New Title", found[0].Title)
        );
    }

    [Fact]
    public async Task InsertAsync_ThenDeleteAsync_EntityNoLongerFound() {
        // Arrange
        var entity = new RepoEntity { Id = "del2", Title = "Transient" };
        await _sut.InsertAsync(new InsertEntitiesRequest<RepoEntity>([entity]), TestContext.Current.CancellationToken);

        // Act
        await _sut.DeleteAsync(new DeleteEntitiesRequest<RepoEntity>([entity]), TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "del2", useWildcard: false).Mandatory().ExactMatch()
        };
        var found = await _sut.SearchAsync<RepoEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(found);
    }

    [Fact]
    public async Task InsertAsync_ThenUpdateAsync_ThenSearchAsync_ReturnsUpdatedValues() {
        // Arrange
        var original = new RepoEntity { Id = "upd3", Title = "First" };
        await _sut.InsertAsync(new InsertEntitiesRequest<RepoEntity>([original]), TestContext.Current.CancellationToken);

        var updated = new RepoEntity { Id = "upd3", Title = "Second" };
        await _sut.UpdateAsync(new UpdateEntitiesRequest<RepoEntity>([updated]), TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "upd3", useWildcard: false).Mandatory().ExactMatch()
        };

        // Act
        var found = await _sut.SearchAsync<RepoEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Multiple(
            () => Assert.Single(found),
            () => Assert.Equal("Second", found[0].Title)
        );
    }

    private static Index CreateIndex(string tempDir) {
        var options = Options.Create(new LuceneOptions { DirectoryName = tempDir });
        var logger = new LoggerMocker<Index>().WithAnyLogLevel().Build();

        var dirMock = new Mock<IDirectory>();
        dirMock.Setup(d => d.Path).Returns(tempDir);
        dirMock.Setup(d => d.Create()).Callback(() => SysDirectory.CreateDirectory(tempDir));

        var fileSystemMock = new Mock<IFileProvider>();
        fileSystemMock
            .Setup(fs => fs.GetDirectory(It.IsAny<string>()))
            .Returns(dirMock.Object);

        return new Index(
            LuceneDefaults.Analyzer,
            fileSystemMock.Object,
            "repo-ext-test",
            options,
            logger
        );
    }
}
