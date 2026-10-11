using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Nameless.IO;
using Nameless.Testing.Tools.Mockers.Logging;

namespace Nameless.Lucene.Repository;

[IntegrationTest]
public class RepositoryImplTests : IDisposable {
    private readonly string _tempDir;
    private readonly Index _index;
    private readonly RepositoryImpl _sut;

    private class RepoTestEntity {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    private class RepoTestEntityMapping : IEntityMapping<RepoTestEntity> {
        public void Map(IEntityDescriptor<RepoTestEntity> descriptor) {
            descriptor.SetID(e => e.Id);
            descriptor.SetProperty(e => e.Title, PropertyOptions.Store);
        }
    }

    public RepositoryImplTests() {
        _tempDir = SysPath.Combine(SysPath.GetTempPath(), $"lucene-repo-tests-{Guid.NewGuid():N}");
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
        services.AddSingleton<IEntityMapping<RepoTestEntity>, RepoTestEntityMapping>();
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
    public async Task InsertAsync_ThenSearchAsync_ReturnsInsertedEntity() {
        // Arrange
        var entity = new RepoTestEntity { Id = "r001", Title = "First Document" };
        var insertRequest = new InsertEntitiesRequest<RepoTestEntity>([entity]);

        // Act
        var response = await _sut.InsertAsync(insertRequest, TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "r001", useWildcard: false).Mandatory().ExactMatch()
        };

        var found = await _sut.SearchAsync<RepoTestEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Equal(1, response.Value.Count),
            () => Assert.Single(found),
            () => Assert.Equal("r001", found[0].Id)
        );
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntities() {
        // Arrange
        var entity = new RepoTestEntity { Id = "r002", Title = "To Be Deleted" };
        var insertRequest = new InsertEntitiesRequest<RepoTestEntity>([entity]);
        await _sut.InsertAsync(insertRequest, TestContext.Current.CancellationToken);

        var deleteRequest = new DeleteEntitiesRequest<RepoTestEntity>([entity]);

        // Act
        var response = await _sut.DeleteAsync(deleteRequest, TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "r002", useWildcard: false).Mandatory().ExactMatch()
        };
        var found = await _sut.SearchAsync<RepoTestEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Empty(found)
        );
    }

    [Fact]
    public async Task UpdateAsync_ChangesEntityValues() {
        // Arrange
        var original = new RepoTestEntity { Id = "r003", Title = "Original Title" };
        var insertRequest = new InsertEntitiesRequest<RepoTestEntity>([original]);
        await _sut.InsertAsync(insertRequest, TestContext.Current.CancellationToken);

        var updated = new RepoTestEntity { Id = "r003", Title = "Updated Title" };
        var updateRequest = new UpdateEntitiesRequest<RepoTestEntity>([updated]);

        // Act
        var response = await _sut.UpdateAsync(updateRequest, TestContext.Current.CancellationToken);

        var searchRequest = new SearchEntitiesRequest {
            Query = qb => qb.WithField("Id", "r003", useWildcard: false).Mandatory().ExactMatch()
        };
        var found = await _sut.SearchAsync<RepoTestEntity>(searchRequest).ToListAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Multiple(
            () => Assert.True(response.Success),
            () => Assert.Single(found),
            () => Assert.Equal("Updated Title", found[0].Title)
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
            "repo-test",
            options,
            logger
        );
    }
}
