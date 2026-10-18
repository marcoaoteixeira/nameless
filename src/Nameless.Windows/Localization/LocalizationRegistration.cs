using Nameless.Registration;

namespace Nameless.Windows.Localization;

/// <summary>
///     Fluent registration options for the localization subsystem.
/// </summary>
public class LocalizationRegistration : AssemblyScanAware<LocalizationRegistration> {
    private static readonly Type LocalizerServiceType = typeof(ILocalizer);

    /// <summary>
    ///     The concrete <see cref="ILocalizer"/> type to register.
    ///     <see langword="null"/> when assembly scanning resolves the type
    ///     instead.
    /// </summary>
    public Type? Localizer {
        get => UseAssemblyScan ? ExecuteAssemblyScan(LocalizerServiceType).SingleOrDefault() : field;
        private set;
    }

    /// <summary>
    ///     Explicitly sets the localizer implementation type.
    /// </summary>
    /// <typeparam name="TLocalizer">
    ///     Type of the localizer.
    /// </typeparam>
    /// <returns>
    ///     This instance for fluent chaining.
    /// </returns>
    public LocalizationRegistration RegisterLocalizer<TLocalizer>() where TLocalizer : ILocalizer {
        return RegisterLocalizer(typeof(TLocalizer));
    }

    /// <summary>
    ///     Explicitly sets the <see cref="ILocalizer"/> implementation type.
    ///     Overrides assembly scanning when called.
    /// </summary>
    /// <param name="type">
    ///     A concrete type that implements <see cref="ILocalizer"/>.
    /// </param>
    /// <returns>
    ///     This instance for fluent chaining.
    /// </returns>
    public LocalizationRegistration RegisterLocalizer(Type type) {
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNotAssignableFrom(type, lhs: LocalizerServiceType);

        Localizer = type;

        return this;
    }
}