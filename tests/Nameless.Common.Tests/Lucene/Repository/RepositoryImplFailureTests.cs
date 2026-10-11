using Microsoft.Extensions.DependencyInjection;
using Moq;
using Nameless.ObjectModel;

namespace Nameless.Lucene.Repository;

[UnitTest]
public class RepositoryImplFailureTests {
    public sealed class FailureEntity {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public sealed class FailureEntityMapping : IEntityMapping<FailureEntity> {
        public void Map(IEntityDescriptor<FailureEntity> descriptor) {
            descriptor.SetID(e => e.Id);
            descriptor.SetProperty(e => e.Title, PropertyOptions.Store);
        }
    }

    private static readonly Result<bool> Ok = true;
    private static readonly Result<bool> Ko = Error.Failure("index failure");
    private static readonly FailureEntity[] Entities = [new() { Id = "1", Title = "t" }];

    private static (RepositoryImpl Sut, Mock<IIndex> Index) CreateSut(bool validMapper = true) {
        var index = new Mock<IIndex>();
        index.Setup(i => i.Insert(It.IsAny<ObjectModel.DocumentCollection>())).Returns(Ok);
        index.Setup(i => i.Delete(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(Ok);
        index.Setup(i => i.Count(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(1);
        index.Setup(i => i.Rollback()).Returns(Ok);
        index.Setup(i => i.SaveChanges()).Returns(Ok);

        var analyzerProvider = new Mock<IAnalyzerProvider>();
        analyzerProvider.Setup(p => p.GetAnalyzer(It.IsAny<string>())).Returns(LuceneDefaults.Analyzer);

        var indexProvider = new Mock<IIndexProvider>();
        indexProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(index.Object);

        IMapper mapper;
        if (validMapper) {
            var services = new ServiceCollection();
            services.AddSingleton<IEntityMapping<FailureEntity>, FailureEntityMapping>();
            mapper = new Mapper(new EntityDescriptorProvider(services.BuildServiceProvider()));
        }
        else {
            mapper = new Mock<IMapper>().Object;
        }

        return (new RepositoryImpl(analyzerProvider.Object, indexProvider.Object, mapper), index);
    }

    private static CancellationToken Cancelled() {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        return cts.Token;
    }

    private static Action<IQueryBuilder> Query => qb => qb.WithField("Id", "1", useWildcard: false).Mandatory().ExactMatch();

    // ── Insert ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task InsertAsync_WhenIndexInsertFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Insert(It.IsAny<ObjectModel.DocumentCollection>())).Returns(Ko);

        // act
        var response = await sut.InsertAsync(new InsertEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task InsertAsync_WhenCancelled_RollsBackAndReportsCancellation() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Rollback()).Returns(Ko);

        // act
        var response = await sut.InsertAsync(new InsertEntitiesRequest<FailureEntity>(Entities), Cancelled());

        // assert
        Assert.Multiple(
            () => Assert.False(response.Success),
            () => Assert.Equal(2, response.Errors.Length),
            () => index.Verify(i => i.Rollback(), Times.Once),
            () => index.Verify(i => i.SaveChanges(), Times.Never)
        );
    }

    [Fact]
    public async Task InsertAsync_WhenSaveFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.SaveChanges()).Returns(Ko);

        // act
        var response = await sut.InsertAsync(new InsertEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenQueryCannotBeBuilt_ReturnsFailure() {
        // arrange
        var (sut, _) = CreateSut(validMapper: false);

        // act
        var response = await sut.DeleteAsync(new DeleteEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteAsync_WhenCountFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Count(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(Error.Failure("count"));

        // act
        var response = await sut.DeleteAsync(new DeleteEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteAsync_WhenDeleteFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Delete(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(Ko);

        // act
        var response = await sut.DeleteAsync(new DeleteEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteAsync_WhenCancelled_RollsBack() {
        // arrange
        var (sut, index) = CreateSut();

        // act
        var response = await sut.DeleteAsync(new DeleteEntitiesRequest<FailureEntity>(Entities), Cancelled());

        // assert
        Assert.Multiple(
            () => Assert.False(response.Success),
            () => index.Verify(i => i.Rollback(), Times.Once)
        );
    }

    [Fact]
    public async Task DeleteAsync_WhenSaveFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.SaveChanges()).Returns(Ko);

        // act
        var response = await sut.DeleteAsync(new DeleteEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    // ── DeleteByQuery ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteByQueryAsync_WhenCountFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Count(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(Error.Failure("count"));

        // act
        var response = await sut.DeleteByQueryAsync(new DeleteEntitiesByQueryRequest { Query = Query }, TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteByQueryAsync_WhenDeleteFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Delete(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(Ko);

        // act
        var response = await sut.DeleteByQueryAsync(new DeleteEntitiesByQueryRequest { Query = Query }, TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteByQueryAsync_WhenCancelled_RollsBack() {
        // arrange
        var (sut, index) = CreateSut();

        // act
        var response = await sut.DeleteByQueryAsync(new DeleteEntitiesByQueryRequest { Query = Query }, Cancelled());

        // assert
        Assert.Multiple(
            () => Assert.False(response.Success),
            () => index.Verify(i => i.Rollback(), Times.Once)
        );
    }

    [Fact]
    public async Task DeleteByQueryAsync_WhenSaveFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.SaveChanges()).Returns(Ko);

        // act
        var response = await sut.DeleteByQueryAsync(new DeleteEntitiesByQueryRequest { Query = Query }, TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteByQueryAsync_OnSuccess_ReturnsCount() {
        // arrange
        var (sut, _) = CreateSut();

        // act
        var response = await sut.DeleteByQueryAsync(new DeleteEntitiesByQueryRequest { Query = Query }, TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(1, response.Value.Count);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WhenQueryCannotBeBuilt_ReturnsFailure() {
        // arrange
        var (sut, _) = CreateSut(validMapper: false);

        // act
        var response = await sut.UpdateAsync(new UpdateEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateAsync_WhenDeleteFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Delete(It.IsAny<global::Lucene.Net.Search.Query>())).Returns(Ko);

        // act
        var response = await sut.UpdateAsync(new UpdateEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateAsync_WhenInsertFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.Insert(It.IsAny<ObjectModel.DocumentCollection>())).Returns(Ko);

        // act
        var response = await sut.UpdateAsync(new UpdateEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateAsync_WhenCancelled_RollsBack() {
        // arrange
        var (sut, index) = CreateSut();

        // act
        var response = await sut.UpdateAsync(new UpdateEntitiesRequest<FailureEntity>(Entities), Cancelled());

        // assert
        Assert.Multiple(
            () => Assert.False(response.Success),
            () => index.Verify(i => i.Rollback(), Times.Once)
        );
    }

    [Fact]
    public async Task UpdateAsync_WhenSaveFails_ReturnsFailure() {
        // arrange
        var (sut, index) = CreateSut();
        index.Setup(i => i.SaveChanges()).Returns(Ko);

        // act
        var response = await sut.UpdateAsync(new UpdateEntitiesRequest<FailureEntity>(Entities), TestContext.Current.CancellationToken);

        // assert
        Assert.False(response.Success);
    }
}
