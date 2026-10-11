using System.Reflection;

namespace Nameless.Registration;

/// <summary>
///     Base class for automatic registration.
/// </summary>
/// <typeparam name="TSelf">
///     The type of the registration.
/// </typeparam>
public abstract class AssemblyScanAware<TSelf> where TSelf : AssemblyScanAware<TSelf>, new() {
    private readonly HashSet<Assembly> _assemblies = [];

    /// <summary>
    ///     Whether it should use assembly scan to locate implementations.
    /// </summary>
    public bool UseAssemblyScan { get; private set; } = true;

    /// <summary>
    ///     Gets the registered assemblies.
    /// </summary>
    public IReadOnlyCollection<Assembly> Assemblies => _assemblies;

    /// <summary>
    ///     Sets whether it should use assembly scan to locate the exported
    ///     types from all defined assemblies.
    /// </summary>
    /// <param name="value">
    ///     The value.
    /// </param>
    /// <returns>
    ///     The current instance of <typeparamref name="TSelf"/> so other
    ///     actions can be chained.
    /// </returns>
    public TSelf WithUseAssemblyScan(bool value) {
        UseAssemblyScan = value;

        return (TSelf)this;
    }

    /// <summary>
    ///     Includes the assembly associated to the type.
    /// </summary>
    /// <typeparam name="TType">
    ///     The type.
    /// </typeparam>
    /// <returns>
    ///     The current <see cref="AssemblyScanAware{TSelf}"/> instance.
    /// </returns>
    public TSelf WithAssemblyFrom<TType>() {
        return WithAssemblies(typeof(TType).Assembly);
    }

    /// <summary>
    ///     Includes the specified assemblies.
    /// </summary>
    /// <param name="assemblies">
    ///     The assemblies.
    /// </param>
    /// <returns>
    ///     The current <see cref="AssemblyScanAware{TSelf}"/> instance.
    /// </returns>
    public TSelf WithAssemblies(params IEnumerable<Assembly> assemblies) {
        foreach (var assembly in assemblies) {
            _assemblies.Add(assembly);
        }

        return (TSelf)this;
    }

    /// <summary>
    ///     Executes the assembly scan over the included assemblies.
    /// </summary>
    /// <param name="service">
    ///     The base type to scan for.
    /// </param>
    /// <param name="includeGenericTypeDefinition">
    ///     Whether it should include open generic types in the scan.
    /// </param>
    /// <returns>
    ///     A <see cref="IReadOnlyCollection{T}"/> of types that implements
    ///     the <paramref name="service"/>.
    /// </returns>
    protected Type[] ExecuteAssemblyScan(Type service, bool includeGenericTypeDefinition = false) {
        var assemblies = Assemblies.Count == 0
            ? [typeof(AssemblyScanAware<>).Assembly]
            : Assemblies;

        var result = assemblies.GetImplementations([service])
                               .Where(type => !IgnoreAssemblyScanAttribute.IsPresent(type))
                               .Where(item => includeGenericTypeDefinition ? includeGenericTypeDefinition : !item.IsGenericTypeDefinition);

        return [.. result];
    }
}
