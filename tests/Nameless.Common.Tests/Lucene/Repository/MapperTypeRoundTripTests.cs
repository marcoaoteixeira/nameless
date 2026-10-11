using Microsoft.Extensions.DependencyInjection;

namespace Nameless.Lucene.Repository;

public enum RoundTripKind {
    None,
    Second
}

public sealed class RoundTripEntity {
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Flag { get; set; }
    public char Letter { get; set; }
    public sbyte SByteValue { get; set; }
    public byte ByteValue { get; set; }
    public short ShortValue { get; set; }
    public ushort UShortValue { get; set; }
    public int IntValue { get; set; }
    public uint UIntValue { get; set; }
    public long LongValue { get; set; }
    public ulong ULongValue { get; set; }
    public float FloatValue { get; set; }
    public double DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public Guid GuidValue { get; set; }
    public DateOnly DateOnlyValue { get; set; }
    public TimeOnly TimeOnlyValue { get; set; }
    public TimeSpan TimeSpanValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public RoundTripKind EnumValue { get; set; }
}

public sealed class RoundTripMapping : IEntityMapping<RoundTripEntity> {
    public void Map(IEntityDescriptor<RoundTripEntity> descriptor) {
        descriptor.SetID(e => e.Id);
        descriptor.SetProperty(e => e.Text, PropertyOptions.Store | PropertyOptions.Analyze | PropertyOptions.Sanitize);
        descriptor.SetProperty(e => e.Flag, PropertyOptions.Store);
        descriptor.SetProperty(e => e.Letter, PropertyOptions.Store);
        descriptor.SetProperty(e => e.SByteValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.ByteValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.ShortValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.UShortValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.IntValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.UIntValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.LongValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.ULongValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.FloatValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.DoubleValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.DecimalValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.DateTimeValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.GuidValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.DateOnlyValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.TimeOnlyValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.TimeSpanValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.DateTimeOffsetValue, PropertyOptions.Store);
        descriptor.SetProperty(e => e.EnumValue, PropertyOptions.Store);
    }
}

[UnitTest]
public class MapperTypeRoundTripTests {
    private static IMapper CreateMapper() {
        var services = new ServiceCollection();
        services.AddSingleton<IEntityMapping<RoundTripEntity>, RoundTripMapping>();

        return new Mapper(new EntityDescriptorProvider(services.BuildServiceProvider()));
    }

    private static RoundTripEntity CreateEntity() {
        return new RoundTripEntity {
            Id = "id-1",
            Text = "<b>bold</b> text",
            Flag = true,
            Letter = 'z',
            SByteValue = 3,
            ByteValue = 200,
            ShortValue = -5,
            UShortValue = 65000,
            IntValue = -42,
            UIntValue = 4_000_000_000,
            LongValue = 9_000_000_000,
            ULongValue = 123456789,
            FloatValue = 1.5f,
            DoubleValue = 2.25,
            DecimalValue = 3.75m,
            DateTimeValue = new DateTime(2026, 5, 6, 7, 8, 9),
            GuidValue = Guid.Parse("11111111-2222-3333-4444-555555555555"),
            DateOnlyValue = new DateOnly(2026, 5, 6),
            TimeOnlyValue = new TimeOnly(7, 8, 9),
            TimeSpanValue = TimeSpan.FromMinutes(90),
            DateTimeOffsetValue = new DateTimeOffset(2026, 5, 6, 7, 8, 9, TimeSpan.Zero),
            EnumValue = RoundTripKind.Second
        };
    }

    [Fact]
    public void MapEntityToDocumentAndBack_PreservesEveryPropertyType() {
        // arrange
        var mapper = CreateMapper();
        var entity = CreateEntity();

        // act
        var document = mapper.Map(entity);
        var actual = mapper.Map<RoundTripEntity>(document);

        // assert
        Assert.Multiple(
            () => Assert.Equal("id-1", actual.Id),
            () => Assert.Equal("bold text", actual.Text),
            () => Assert.True(actual.Flag),
            () => Assert.Equal('z', actual.Letter),
            () => Assert.Equal(entity.SByteValue, actual.SByteValue),
            () => Assert.Equal(entity.ByteValue, actual.ByteValue),
            () => Assert.Equal(entity.ShortValue, actual.ShortValue),
            () => Assert.Equal(entity.UShortValue, actual.UShortValue),
            () => Assert.Equal(entity.IntValue, actual.IntValue),
            () => Assert.Equal(entity.UIntValue, actual.UIntValue),
            () => Assert.Equal(entity.LongValue, actual.LongValue),
            () => Assert.Equal(entity.ULongValue, actual.ULongValue),
            () => Assert.Equal(entity.FloatValue, actual.FloatValue),
            () => Assert.Equal(entity.DoubleValue, actual.DoubleValue),
            () => Assert.Equal(entity.DecimalValue, actual.DecimalValue),
            () => Assert.Equal(entity.DateTimeValue, actual.DateTimeValue),
            () => Assert.Equal(entity.GuidValue, actual.GuidValue),
            () => Assert.Equal(entity.DateOnlyValue, actual.DateOnlyValue),
            () => Assert.Equal(entity.TimeOnlyValue, actual.TimeOnlyValue),
            () => Assert.Equal(entity.TimeSpanValue, actual.TimeSpanValue),
            () => Assert.Equal(entity.DateTimeOffsetValue, actual.DateTimeOffsetValue),
            () => Assert.Equal(entity.EnumValue, actual.EnumValue)
        );
    }
}
