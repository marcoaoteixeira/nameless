
namespace Nameless.Windows.Hosting;

[UnitTest]
public class WinHostFeatureCollectionTests
{
    public class FeatureA : WinHostFeature;

    public class FeatureB : WinHostFeature;

    public class FeatureC : WinHostFeature;

    public class FeatureD : WinHostFeature;

    public class SubFeatureA : FeatureA;

    public class OtherSubFeatureA : FeatureA;

    private static Type[] TypesOf(WinHostFeatureCollection features)
    {
        return [.. features.Select(feature => feature.GetType())];
    }

    private static void LookupFeatureA(WinHostFeatureCollection features, string method)
    {
        switch (method)
        {
            case nameof(WinHostFeatureCollection.AddBefore): features.AddBefore<FeatureA>(new FeatureC()); break;
            case nameof(WinHostFeatureCollection.AddAfter): features.AddAfter<FeatureA>(new FeatureC()); break;
            case nameof(WinHostFeatureCollection.Replace): features.Replace<FeatureA>(new FeatureC()); break;
            case nameof(WinHostFeatureCollection.Remove): features.Remove<FeatureA>(); break;
            case nameof(WinHostFeatureCollection.Get): features.Get<FeatureA>(); break;
            default: throw new ArgumentOutOfRangeException(nameof(method));
        }
    }

    [Fact]
    public void WhenCreated_ThenCollectionIsEmpty()
    {
        // act
        var sut = new WinHostFeatureCollection();

        // assert
        Assert.Empty(sut);
    }

    [Fact]
    public void WhenAdd_ThenAppendFeature()
    {
        // arrange
        var sut = new WinHostFeatureCollection();

        // act
        sut.Add(new FeatureA()).Add(new FeatureB());

        // assert
        Assert.Equal([typeof(FeatureA), typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenAdd_WhenCollectionHasFeatures_ThenAppendAfterLastFeature()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB())
            .AddBefore<FeatureA>(new FeatureC());

        // act
        sut.Add(new FeatureD());

        // assert
        Assert.Equal([typeof(FeatureC), typeof(FeatureA), typeof(FeatureB), typeof(FeatureD)], TypesOf(sut));
    }

    [Fact]
    public void WhenAdd_WhenFeatureTypeAlreadyExists_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => sut.Add(new FeatureA()));

        // assert
        Assert.Contains(nameof(FeatureA), actual.Message);
    }

    [Fact]
    public void WhenAdd_WhenSubclassOfExistingFeature_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => sut.Add(new SubFeatureA()));

        // assert
        Assert.Multiple(
            () => Assert.Contains(nameof(SubFeatureA), actual.Message),
            () => Assert.Contains($"Replace<{nameof(FeatureA)}>", actual.Message),
            () => Assert.Equal([typeof(FeatureA)], TypesOf(sut))
        );
    }

    [Fact]
    public void WhenAdd_WhenBaseOfExistingFeature_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new SubFeatureA());

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => sut.Add(new FeatureA()));

        // assert
        Assert.Multiple(
            () => Assert.Contains(nameof(SubFeatureA), actual.Message),
            () => Assert.Equal([typeof(SubFeatureA)], TypesOf(sut))
        );
    }

    [Fact]
    public void WhenAdd_WhenSiblingOfExistingFeature_ThenAppendFeature()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new SubFeatureA());

        // act
        sut.Add(new OtherSubFeatureA());

        // assert
        Assert.Equal([typeof(SubFeatureA), typeof(OtherSubFeatureA)], TypesOf(sut));
    }

    [Fact]
    public void WhenAdd_WhenFeatureIsNull_ThenThrowArgumentNullException()
    {
        // arrange
        var sut = new WinHostFeatureCollection();

        // act & assert
        Assert.Throws<ArgumentNullException>(() => sut.Add(null!));
    }

    [Fact]
    public void WhenAddBefore_WhenAnchorExists_ThenInsertBeforeAnchor()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act
        sut.AddBefore<FeatureA>(new FeatureC());

        // assert
        Assert.Equal([typeof(FeatureC), typeof(FeatureA), typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenAddBefore_WhenAnchorWasReplacedBySubclass_ThenInsertBeforeSubclass()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB())
            .Replace<FeatureA>(new SubFeatureA());

        // act
        sut.AddBefore<FeatureA>(new FeatureC());

        // assert
        Assert.Equal([typeof(FeatureC), typeof(SubFeatureA), typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenAddBefore_WhenAnchorIsMissing_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => sut.AddBefore<FeatureB>(new FeatureC()));

        // assert
        Assert.Contains(nameof(FeatureB), actual.Message);
    }

    [Theory]
    [InlineData(nameof(WinHostFeatureCollection.AddBefore))]
    [InlineData(nameof(WinHostFeatureCollection.AddAfter))]
    [InlineData(nameof(WinHostFeatureCollection.Replace))]
    [InlineData(nameof(WinHostFeatureCollection.Remove))]
    [InlineData(nameof(WinHostFeatureCollection.Get))]
    public void WhenLookup_WhenTypeIsAmbiguous_ThenThrowInvalidOperationException(string method)
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new SubFeatureA())
            .Add(new OtherSubFeatureA());

        // act
        var actual = Assert.Throws<InvalidOperationException>(() => LookupFeatureA(sut, method));

        // assert
        Assert.Multiple(
            () => Assert.Contains(nameof(SubFeatureA), actual.Message),
            () => Assert.Contains(nameof(OtherSubFeatureA), actual.Message)
        );
    }

    [Fact]
    public void WhenAddBefore_WhenFeatureIsRelatedToExistingFeature_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.AddBefore<FeatureB>(new SubFeatureA()));
    }

    [Fact]
    public void WhenAddAfter_WhenFeatureIsRelatedToExistingFeature_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.AddAfter<FeatureB>(new SubFeatureA()));
    }

    [Fact]
    public void WhenAddAfter_WhenFeatureTypeAlreadyExists_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.AddAfter<FeatureA>(new FeatureB()));
    }

    [Fact]
    public void WhenAddBefore_WhenFeatureTypeAlreadyExists_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.AddBefore<FeatureA>(new FeatureB()));
    }

    [Fact]
    public void WhenAddBefore_WhenFeatureIsNull_ThenThrowArgumentNullException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act & assert
        Assert.Throws<ArgumentNullException>(() => sut.AddBefore<FeatureA>(null!));
    }

    [Fact]
    public void WhenAddAfter_WhenAnchorExists_ThenInsertAfterAnchor()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act
        sut.AddAfter<FeatureA>(new FeatureC());

        // assert
        Assert.Equal([typeof(FeatureA), typeof(FeatureC), typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenAddAfter_WhenAnchorIsLast_ThenAppendFeature()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act
        sut.AddAfter<FeatureB>(new FeatureC());

        // assert
        Assert.Equal([typeof(FeatureA), typeof(FeatureB), typeof(FeatureC)], TypesOf(sut));
    }

    [Fact]
    public void WhenAddAfter_WhenAnchorIsMissing_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.AddAfter<FeatureB>(new FeatureC()));
    }

    [Fact]
    public void WhenAddAfter_WhenFeatureIsNull_ThenThrowArgumentNullException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act & assert
        Assert.Throws<ArgumentNullException>(() => sut.AddAfter<FeatureA>(null!));
    }

    [Fact]
    public void WhenReplace_WhenFeatureExists_ThenKeepPosition()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB())
            .Add(new FeatureC());

        // act
        sut.Replace<FeatureB>(new FeatureD());

        // assert
        Assert.Equal([typeof(FeatureA), typeof(FeatureD), typeof(FeatureC)], TypesOf(sut));
    }

    [Fact]
    public void WhenReplace_WhenReplacementIsSubclassOfReplacedFeature_ThenKeepPosition()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act
        sut.Replace<FeatureA>(new SubFeatureA());

        // assert
        Assert.Equal([typeof(SubFeatureA), typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenReplace_WhenReplacementIsRelatedToAnotherFeature_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.Replace<FeatureB>(new SubFeatureA()));
    }

    [Fact]
    public void WhenReplace_WhenReplacementHasSameType_ThenReplaceInstance()
    {
        // arrange
        var replacement = new FeatureA();
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act
        sut.Replace<FeatureA>(replacement);

        // assert
        Assert.Same(replacement, Assert.Single(sut));
    }

    [Fact]
    public void WhenReplace_WhenReplacementTypeExistsElsewhere_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.Replace<FeatureA>(new FeatureB()));
    }

    [Fact]
    public void WhenReplace_WhenFeatureIsMissing_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act & assert
        Assert.Throws<InvalidOperationException>(() => sut.Replace<FeatureB>(new FeatureC()));
    }

    [Fact]
    public void WhenReplace_WhenFeatureIsNull_ThenThrowArgumentNullException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act & assert
        Assert.Throws<ArgumentNullException>(() => sut.Replace<FeatureA>(null!));
    }

    [Fact]
    public void WhenRemove_WhenFeatureExists_ThenRemoveFeature()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act
        sut.Remove<FeatureA>();

        // assert
        Assert.Equal([typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenRemove_WhenFeatureWasReplacedBySubclass_ThenRemoveSubclass()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureB())
            .Add(new SubFeatureA());

        // act
        sut.Remove<FeatureA>();

        // assert
        Assert.Equal([typeof(FeatureB)], TypesOf(sut));
    }

    [Fact]
    public void WhenRemove_WhenFeatureIsMissing_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act & assert
        Assert.Throws<InvalidOperationException>(sut.Remove<FeatureB>);
    }

    [Fact]
    public void WhenGet_WhenFeatureExists_ThenReturnInstance()
    {
        // arrange
        var feature = new FeatureB();
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(feature);

        // act
        var actual = sut.Get<FeatureB>();

        // assert
        Assert.Same(feature, actual);
    }

    [Fact]
    public void WhenGet_WhenFeatureWasReplacedBySubclass_ThenReturnSubclassInstance()
    {
        // arrange
        var feature = new SubFeatureA();
        var sut = new WinHostFeatureCollection().Add(feature);

        // act
        var actual = sut.Get<FeatureA>();

        // assert
        Assert.Same(feature, actual);
    }

    [Fact]
    public void WhenGet_WhenFeatureIsMissing_ThenThrowInvalidOperationException()
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act
        var actual = Assert.Throws<InvalidOperationException>(sut.Get<FeatureB>);

        // assert
        Assert.Contains(nameof(FeatureB), actual.Message);
    }

    [Theory]
    [InlineData(nameof(WinHostFeatureCollection.Add))]
    [InlineData(nameof(WinHostFeatureCollection.AddBefore))]
    [InlineData(nameof(WinHostFeatureCollection.AddAfter))]
    [InlineData(nameof(WinHostFeatureCollection.Replace))]
    [InlineData(nameof(WinHostFeatureCollection.Remove))]
    public void WhenMutate_ThenReturnSameCollection(string method)
    {
        // arrange
        var sut = new WinHostFeatureCollection().Add(new FeatureA());

        // act
        var actual = method switch
        {
            nameof(WinHostFeatureCollection.Add) => sut.Add(new FeatureB()),
            nameof(WinHostFeatureCollection.AddBefore) => sut.AddBefore<FeatureA>(new FeatureB()),
            nameof(WinHostFeatureCollection.AddAfter) => sut.AddAfter<FeatureA>(new FeatureB()),
            nameof(WinHostFeatureCollection.Replace) => sut.Replace<FeatureA>(new FeatureB()),
            nameof(WinHostFeatureCollection.Remove) => sut.Remove<FeatureA>(),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        // assert
        Assert.Same(sut, actual);
    }

    [Fact]
    public void WhenCount_ThenReturnNumberOfFeatures()
    {
        // arrange
        var sut = new WinHostFeatureCollection()
            .Add(new FeatureA())
            .Add(new FeatureB());

        // act
        var actual = sut.Count;

        // assert
        Assert.Equal(2, actual);
    }
}
