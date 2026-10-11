using Lucene.Net.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace Nameless.Lucene.Repository.Extensions;

// Tests for public EntityDescriptorExtensions and the mapper round-trips
// that exercise the internal LuceneDocumentExtensions / PropertyDescriptorExtensions.
[UnitTest]
public class LuceneExtensionsTests {
    // ── Test entity covering all supported Lucene field types ────────────────

    private class AllTypesEntity {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public long LongValue { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public Guid Guid { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }

    private class AllTypesMapping : IEntityMapping<AllTypesEntity> {
        public void Map(IEntityDescriptor<AllTypesEntity> descriptor) {
            descriptor.SetID(e => e.Id);
            descriptor.SetProperty(e => e.Name, PropertyOptions.Store);
            descriptor.SetProperty(e => e.Age, PropertyOptions.Store);
            descriptor.SetProperty(e => e.LongValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.FloatValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.DoubleValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.IsActive, PropertyOptions.Store);
            descriptor.SetProperty(e => e.Created, PropertyOptions.Store);
            descriptor.SetProperty(e => e.Guid, PropertyOptions.Store);
            descriptor.SetProperty(e => e.DayOfWeek, PropertyOptions.Store);
        }
    }

    private static IMapper CreateMapper<TEntity, TMapping>()
        where TEntity : class
        where TMapping : class, IEntityMapping<TEntity> {
        var services = new ServiceCollection();
        services.AddSingleton<IEntityMapping<TEntity>, TMapping>();
        var provider = services.BuildServiceProvider();
        return new Mapper(new EntityDescriptorProvider(provider));
    }

    // ── EntityDescriptorExtensions.Property<T> ───────────────────────────────

    private class SimpleEntity {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class SimpleEntityMapping : IEntityMapping<SimpleEntity> {
        public void Map(IEntityDescriptor<SimpleEntity> descriptor) {
            descriptor.SetID(e => e.Id);
            // Use the public extension method under test
            descriptor.Property(e => e.Name);
        }
    }

    [Fact]
    public void EntityDescriptorExtensions_Property_RegistersPropertyWithStoreOption() {
        // arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEntityMapping<SimpleEntity>, SimpleEntityMapping>();
        var provider = services.BuildServiceProvider();

        var descProvider = new EntityDescriptorProvider(provider);
        var descriptor = descProvider.GetDescriptor<SimpleEntity>();

        // act
        var nameProp = descriptor.Properties.SingleOrDefault(p => p.Name == "Name");

        // assert
        Assert.NotNull(nameProp);
        Assert.True(nameProp.Options.HasFlag(PropertyOptions.Store));
    }

    // ── PropertyDescriptorExtensions.TryCreateField (via Mapper.Map entity→doc) ──

    [Fact]
    public void Mapper_MapEntityToDocument_StringFieldIsPresent() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var entity = new AllTypesEntity { Id = "id1", Name = "Alice" };

        // act
        var doc = mapper.Map(entity);

        // assert
        Assert.Multiple(
            () => Assert.NotNull(doc.GetField("Name")),
            () => Assert.Equal("Alice", doc.GetField("Name").GetStringValue())
        );
    }

    [Fact]
    public void Mapper_MapEntityToDocument_IntFieldIsPresent() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var entity = new AllTypesEntity { Id = "id2", Age = 30 };

        // act
        var doc = mapper.Map(entity);

        // assert
        Assert.NotNull(doc.GetField("Age"));
    }

    [Fact]
    public void Mapper_MapEntityToDocument_BoolFieldIsPresent() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var entity = new AllTypesEntity { Id = "id3", IsActive = true };

        // act
        var doc = mapper.Map(entity);

        // assert
        Assert.NotNull(doc.GetField("IsActive"));
    }

    [Fact]
    public void Mapper_MapEntityToDocument_GuidFieldIsPresent() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var guid = Guid.NewGuid();
        var entity = new AllTypesEntity { Id = "id4", Guid = guid };

        // act
        var doc = mapper.Map(entity);

        // assert
        var field = doc.GetField("Guid");
        Assert.Multiple(
            () => Assert.NotNull(field),
            () => Assert.Equal(guid.ToString(), field.GetStringValue())
        );
    }

    [Fact]
    public void Mapper_MapEntityToDocument_DateTimeFieldIsPresent() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var now = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        var entity = new AllTypesEntity { Id = "id5", Created = now };

        // act
        var doc = mapper.Map(entity);

        // assert
        Assert.NotNull(doc.GetField("Created"));
    }

    [Fact]
    public void Mapper_MapEntityToDocument_EnumFieldIsPresent() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var entity = new AllTypesEntity { Id = "id6", DayOfWeek = DayOfWeek.Wednesday };

        // act
        var doc = mapper.Map(entity);

        // assert
        var field = doc.GetField("DayOfWeek");
        Assert.Multiple(
            () => Assert.NotNull(field),
            () => Assert.Equal(DayOfWeek.Wednesday.ToString(), field.GetStringValue())
        );
    }

    // ── LuceneDocumentExtensions.TryGetValue (via Mapper.Map doc→entity) ──────

    [Fact]
    public void Mapper_MapDocumentToEntity_StringFieldIsPopulated() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var doc = new Document();
        doc.Add(new StringField("Id", "doc-id", Field.Store.YES));
        doc.Add(new StringField("Name", "Bob", Field.Store.YES));

        // act
        var entity = mapper.Map<AllTypesEntity>(doc);

        // assert
        Assert.Equal("Bob", entity.Name);
    }

    [Fact]
    public void Mapper_MapDocumentToEntity_IntFieldIsPopulated() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var doc = new Document();
        doc.Add(new StringField("Id", "doc-id", Field.Store.YES));
        doc.Add(new Int32Field("Age", 42, Field.Store.YES));

        // act
        var entity = mapper.Map<AllTypesEntity>(doc);

        // assert
        Assert.Equal(42, entity.Age);
    }

    [Fact]
    public void Mapper_MapDocumentToEntity_BoolFieldIsPopulated() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var doc = new Document();
        doc.Add(new StringField("Id", "doc-id", Field.Store.YES));
        // bool is stored as Int32 (1 = true)
        doc.Add(new Int32Field("IsActive", 1, Field.Store.YES));

        // act
        var entity = mapper.Map<AllTypesEntity>(doc);

        // assert
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void Mapper_RoundTrip_AllCommonTypes_PreservesValues() {
        // arrange
        var mapper = CreateMapper<AllTypesEntity, AllTypesMapping>();
        var original = new AllTypesEntity {
            Id = "rt-id",
            Name = "RoundTrip",
            Age = 99,
            IsActive = true
        };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<AllTypesEntity>(doc);

        // assert
        Assert.Multiple(
            () => Assert.Equal(original.Id, restored.Id),
            () => Assert.Equal(original.Name, restored.Name),
            () => Assert.Equal(original.Age, restored.Age),
            () => Assert.Equal(original.IsActive, restored.IsActive)
        );
    }

    // ── EntityDescriptorExtensions.HasID (internal, exercised via public SetID) ──

    [Fact]
    public void EntityDescriptor_SetID_CalledTwice_ThrowsInvalidOperationException() {
        // arrange
        var descriptor = new EntityDescriptor<SimpleEntity>();

        // act
        descriptor.SetID(e => e.Id);

        // assert — calling SetID again must throw because HasID is already true
        Assert.Throws<InvalidOperationException>(() => descriptor.SetID(e => e.Name));
    }

    // ── Extended type coverage for LuceneDocumentExtensions / PropertyDescriptorExtensions ──

    private class ExtendedTypesEntity {
        public string Id { get; set; } = string.Empty;
        public decimal DecimalValue { get; set; }
        public float FloatValue { get; set; }
        public long LongValue { get; set; }
        public short ShortValue { get; set; }
        public byte ByteValue { get; set; }
        public DateOnly DateOnlyValue { get; set; }
        public TimeOnly TimeOnlyValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
    }

    private class ExtendedTypesMapping : IEntityMapping<ExtendedTypesEntity> {
        public void Map(IEntityDescriptor<ExtendedTypesEntity> descriptor) {
            descriptor.SetID(e => e.Id);
            descriptor.SetProperty(e => e.DecimalValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.FloatValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.LongValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.ShortValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.ByteValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.DateOnlyValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.TimeOnlyValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.TimeSpanValue, PropertyOptions.Store);
            descriptor.SetProperty(e => e.DateTimeOffsetValue, PropertyOptions.Store);
        }
    }

    [Fact]
    public void Mapper_RoundTrip_DecimalValue_PreservesApproximateValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "dec-1", DecimalValue = 12.34m };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert — stored via double conversion so precision is approximate
        Assert.Equal((double)original.DecimalValue, (double)restored.DecimalValue, precision: 5);
    }

    [Fact]
    public void Mapper_RoundTrip_FloatValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "flt-1", FloatValue = 3.14f };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.FloatValue, restored.FloatValue, precision: 3);
    }

    [Fact]
    public void Mapper_RoundTrip_LongValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "lng-1", LongValue = 9_876_543_210L };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.LongValue, restored.LongValue);
    }

    [Fact]
    public void Mapper_RoundTrip_ShortValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "shrt-1", ShortValue = 42 };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.ShortValue, restored.ShortValue);
    }

    [Fact]
    public void Mapper_RoundTrip_ByteValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "byte-1", ByteValue = 255 };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.ByteValue, restored.ByteValue);
    }

    [Fact]
    public void Mapper_RoundTrip_DateOnlyValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "do-1", DateOnlyValue = new DateOnly(2025, 6, 15) };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.DateOnlyValue, restored.DateOnlyValue);
    }

    [Fact]
    public void Mapper_RoundTrip_TimeOnlyValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "to-1", TimeOnlyValue = new TimeOnly(14, 30, 0) };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.TimeOnlyValue, restored.TimeOnlyValue);
    }

    [Fact]
    public void Mapper_RoundTrip_TimeSpanValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity { Id = "ts-1", TimeSpanValue = TimeSpan.FromHours(2.5) };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.TimeSpanValue, restored.TimeSpanValue);
    }

    [Fact]
    public void Mapper_RoundTrip_DateTimeOffsetValue_PreservesValue() {
        // arrange
        var mapper = CreateMapper<ExtendedTypesEntity, ExtendedTypesMapping>();
        var original = new ExtendedTypesEntity {
            Id = "dto-1",
            DateTimeOffsetValue = new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero)
        };

        // act
        var doc = mapper.Map(original);
        var restored = mapper.Map<ExtendedTypesEntity>(doc);

        // assert
        Assert.Equal(original.DateTimeOffsetValue, restored.DateTimeOffsetValue);
    }
}
