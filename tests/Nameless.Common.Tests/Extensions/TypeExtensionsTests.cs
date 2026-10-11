namespace Nameless.Extensions;

[UnitTest]
public class TypeExtensionsTests {
    // ─── IsOpenGeneric ───────────────────────────────────────────────────────

    [Fact]
    public void IsOpenGeneric_ForOpenGenericType_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(List<>).IsOpenGeneric);
    }

    [Fact]
    public void IsOpenGeneric_ForClosedGenericType_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(List<int>).IsOpenGeneric);
    }

    [Fact]
    public void IsOpenGeneric_ForNonGenericType_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(string).IsOpenGeneric);
    }

    // ─── IsConcrete ──────────────────────────────────────────────────────────

    [Fact]
    public void IsConcrete_ForConcreteClass_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(string).IsConcrete);
    }

    [Fact]
    public void IsConcrete_ForInterface_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(IDisposable).IsConcrete);
    }

    [Fact]
    public void IsConcrete_ForAbstractClass_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(AbstractBase).IsConcrete);
    }

    // ─── IsNullable ──────────────────────────────────────────────────────────

    [Fact]
    public void IsNullable_ForNullableValueType_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(int?).IsNullable);
    }

    [Fact]
    public void IsNullable_ForNonNullableValueType_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(int).IsNullable);
    }

    // ─── AllowNull ───────────────────────────────────────────────────────────

    [Fact]
    public void AllowNull_ForNullableValueType_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(int?).AllowNull);
    }

    [Fact]
    public void AllowNull_ForReferenceType_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(string).AllowNull);
    }

    [Fact]
    public void AllowNull_ForNonNullableValueType_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(int).AllowNull);
    }

    // ─── IsAssignableFromGeneric ─────────────────────────────────────────────

    [Fact]
    public void IsAssignableFromGeneric_WhenAssignable_ReturnsTrue() {
        // act
        var result = typeof(IEnumerable<>).IsAssignableFromGeneric(typeof(List<int>));

        // assert
        Assert.True(result);
    }

    [Fact]
    public void IsAssignableFromGeneric_WhenNotAssignable_ReturnsFalse() {
        // act — use a non-enumerable concrete type
        var result = typeof(IEnumerable<>).IsAssignableFromGeneric(typeof(Uri));

        // assert
        Assert.False(result);
    }

    // ─── HasInterface ────────────────────────────────────────────────────────

    [Fact]
    public void HasInterface_WhenTypeImplementsInterface_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(MemoryStream).HasInterface<IDisposable>());
    }

    [Fact]
    public void HasInterface_WhenTypeDoesNotImplementInterface_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(object).HasInterface<IDisposable>());
    }

    // ─── HasAttribute ────────────────────────────────────────────────────────

    [Fact]
    public void HasAttribute_WhenTypeHasAttribute_ReturnsTrue() {
        // act & assert
#pragma warning disable CS0618
        Assert.True(typeof(ObsoleteType).HasAttribute<ObsoleteAttribute>());
#pragma warning restore CS0618
    }

    [Fact]
    public void HasAttribute_WhenTypeDoesNotHaveAttribute_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(string).HasAttribute<ObsoleteAttribute>());
    }

    // ─── HasParameterlessConstructor ─────────────────────────────────────────

    [Fact]
    public void HasParameterlessConstructor_ForTypeWithDefaultCtor_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(List<int>).HasParameterlessConstructor());
    }

    [Fact]
    public void HasParameterlessConstructor_ForTypeWithoutDefaultCtor_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(NoDefaultCtor).HasParameterlessConstructor());
    }

    // ─── GetPrettyName ───────────────────────────────────────────────────────

    [Fact]
    public void GetPrettyName_ForNonGenericType_ReturnsTypeName() {
        // act
        var result = typeof(string).GetPrettyName();

        // assert
        Assert.Equal("String", result);
    }

    [Fact]
    public void GetPrettyName_ForClosedGenericType_ReturnsNameWithTypeArguments() {
        // act
        var result = typeof(List<int>).GetPrettyName();

        // assert
        Assert.Equal("List<Int32>", result);
    }

    [Fact]
    public void GetPrettyName_ForOpenGenericType_ReturnsNameWithTypeParameterName() {
        // act — open generic parameter names are preserved (e.g. "T"), not empty
        var result = typeof(List<>).GetPrettyName();

        // assert: contains the base name and angle brackets
        Assert.Multiple(
            () => Assert.StartsWith("List<", result),
            () => Assert.EndsWith(">", result)
        );
    }

    [Fact]
    public void GetPrettyName_ForNonGenericTypeNestedInGenericType_ReturnsTypeName() {
        // act — the nested type is generic only through its declaring type,
        // so its name carries no arity suffix.
        var result = typeof(PrettyNameOuter<>.Inner).GetPrettyName();

        // assert
        Assert.Equal("Inner", result);
    }

    // ─── CanInstantiate ──────────────────────────────────────────────────────

    [Fact]
    public void CanInstantiate_ForConcreteClosedGenericWithDefaultCtor_ReturnsTrue() {
        // act & assert
        Assert.True(typeof(List<int>).CanInstantiate());
    }

    [Fact]
    public void CanInstantiate_ForAbstractClass_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(AbstractBase).CanInstantiate());
    }

    [Fact]
    public void CanInstantiate_ForInterface_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(IDisposable).CanInstantiate());
    }

    [Fact]
    public void CanInstantiate_ForOpenGenericType_ReturnsFalse() {
        // act & assert
        Assert.False(typeof(List<>).CanInstantiate());
    }

    // ─── GetInterfacesThatCloses ─────────────────────────────────────────────

    [Fact]
    public void GetInterfacesThatCloses_WhenTypeImplementsClosedGeneric_ReturnsInterface() {
        // act
        var result = typeof(MyList).GetTypesThatCloses(typeof(IEnumerable<>)).ToList();

        // assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetInterfacesThatCloses_WithNonGenericDefinition_ReturnsEmpty() {
        // act
        var result = typeof(MyList).GetTypesThatCloses(typeof(IDisposable)).ToList();

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetInterfacesThatCloses_WithInterface_ReturnsEmpty() {
        // act
        var result = typeof(IDisposable).GetTypesThatCloses(typeof(IEnumerable<>)).ToList();

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetInterfacesThatCloses_WithAbstractClass_ReturnsEmpty() {
        // act
        var result = typeof(AbstractBase).GetTypesThatCloses(typeof(IEnumerable<>)).ToList();

        // assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetInterfacesThatCloses_WithGenericClassDefinition_ReturnsClosedBaseType() {
        // MyList : List<int>; asking which part closes List<> (a class, not an interface)
        var result = typeof(MyList).GetTypesThatCloses(typeof(List<>)).ToList();

        Assert.NotEmpty(result);
    }

    // ─── FixTypeReference ────────────────────────────────────────────────────

    [Fact]
    public void FixTypeReference_ForConcreteType_ReturnsSameType() {
        // act
        var result = typeof(string).FixTypeReference();

        // assert
        Assert.Equal(typeof(string), result);
    }

    // ─── IsAssignableFromGeneric (null base type path) ──────────────────────

    [Fact]
    public void IsAssignableFromGeneric_WhenTypeIsNull_ReturnsFalse() {
        // act
        var result = typeof(IEnumerable<>).IsAssignableFromGeneric(null);

        // assert
        Assert.False(result);
    }

    // ─── test doubles ────────────────────────────────────────────────────────

    private abstract class AbstractBase { }
#pragma warning disable CS9113
    private sealed class NoDefaultCtor(int value) { }
#pragma warning restore CS9113

    [Obsolete("test")]
    private sealed class ObsoleteType { }

    private sealed class MyList : List<int> { }
}

public class PrettyNameOuter<T> {
    public class Inner;
}
