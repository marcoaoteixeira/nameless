using Microsoft.Extensions.Logging;
using Nameless.Lucene;
using Nameless.Logging;
using Nameless.ObjectModel;
using Moq;

namespace Nameless.Helpers;

public class ConstraintBaseClass;

public sealed class ConstraintDerived : ConstraintBaseClass;

public sealed class ConstraintBaseHolder<T> where T : ConstraintBaseClass;

public sealed class ConstraintOpenGenericHolder<T> where T : IEnumerable<int>;

public sealed class ConstraintIntList : List<int>;

public sealed class ConstraintSelfHolder<TKey, TValue>
    where TKey : class, new()
    where TValue : IComparable<TKey>;

public sealed class ConstraintComparableKey : IComparable<ConstraintComparableKey> {
    public int CompareTo(ConstraintComparableKey? other) {
        return 0;
    }
}

[UnitTest]
public class GenericTypeHelperMoreConstraintTests {
    private static readonly System.Reflection.Assembly TestAssembly = typeof(GenericTypeHelperMoreConstraintTests).Assembly;

    [Fact]
    public void GetArgumentsThatCloses_WithBaseClassConstraint_IncludesBaseAndDerived() {
        // act
        var combinations = GenericTypeHelper.GetArgumentsThatCloses(typeof(ConstraintBaseHolder<>), TestAssembly).ToList();

        // assert
        Assert.Multiple(
            () => Assert.Contains(combinations, c => c[0] == typeof(ConstraintDerived)),
            () => Assert.Contains(combinations, c => c[0] == typeof(ConstraintBaseClass))
        );
    }

    [Fact]
    public void GetArgumentsThatCloses_WithOpenGenericConstraint_FindsImplementations() {
        // act
        var combinations = GenericTypeHelper.GetArgumentsThatCloses(typeof(ConstraintOpenGenericHolder<>), TestAssembly).ToList();

        // assert
        Assert.Contains(combinations, c => c[0] == typeof(ConstraintIntList));
    }

    [Fact]
    public void GetArgumentsThatCloses_WithSelfReferencingConstraint_ResolvesGenericParameters() {
        // act
        var combinations = GenericTypeHelper.GetArgumentsThatCloses(typeof(ConstraintSelfHolder<,>), TestAssembly).ToList();

        // assert
        Assert.Contains(combinations, c => c[0] == typeof(ConstraintComparableKey) && c[1] == typeof(ConstraintComparableKey));
    }

    [Fact]
    public void StartStopwatchLogger_WithRealGenericLogger_UsesCategoryTypeName() {
        // arrange
        using var factory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug));
        var logger = factory.CreateLogger<GenericTypeHelperMoreConstraintTests>();

        // act
        var exception = Record.Exception(() => { using var _ = logger.StartStopwatchLogger(); });

        // assert
        Assert.Null(exception);
    }

    [Fact]
    public void AnalyzerProvider_WithBlankIndexName_ReturnsDefaultAnalyzer() {
        // arrange
        var selector = new Mock<IAnalyzerSelector>();
        var sut = new AnalyzerProvider([selector.Object]);

        // act
        var actual = sut.GetAnalyzer("  ");

        // assert
        Assert.Multiple(
            () => Assert.NotNull(actual),
            () => selector.Verify(s => s.GetAnalyzer(It.IsAny<string>()), Times.Never)
        );
    }
}
