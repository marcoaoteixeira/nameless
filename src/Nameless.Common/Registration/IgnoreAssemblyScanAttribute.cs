using System.Reflection;

namespace Nameless.Registration;

/// <summary>
///     Indicates that the annotated type should not be included in the
///     assembly scan process of <see cref="AssemblyScanAware{TSelf}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class IgnoreAssemblyScanAttribute : Attribute {
    /// <summary>
    ///     Whether if the attribute is present in the type.
    /// </summary>
    /// <param name="type">
    ///     The type.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> if the attribute is present;
    ///     otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsPresent(Type type) {
        return type.GetCustomAttribute<IgnoreAssemblyScanAttribute>(inherit: false) is not null;
    }
}
