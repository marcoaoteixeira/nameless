using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting;

/// <summary>
///     Represents a feature of the Windows host, like logging, navigation
///     or localization, that <see cref="WinHostFactory"/> registers when
///     building the host.
/// </summary>
/// <remarks>
///     Features are kept in a <see cref="WinHostFeatureCollection"/>,
///     configured through <see cref="WinHostFactory.CreateWinApp"/>. The factory
///     calls <see cref="Register"/> on every feature, in collection order,
///     before building the host. Services registered later override the
///     ones registered earlier, so a feature added last can replace any
///     built-in registration.
/// </remarks>
public abstract class WinHostFeature {
    /// <summary>
    ///     Gets the feature name.
    /// </summary>
    public virtual string Name => GetType().Name;

    /// <summary>
    ///     Registers the services required by the feature.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="WinHostBuilder"/> being configured.
    /// </param>
    /// <param name="context">
    ///     The <see cref="WinHostFeatureContext"/> shared by every feature.
    /// </param>
    public virtual void Register(WinHostBuilder builder, WinHostFeatureContext context) { }
}

/// <summary>
///     Represents a <see cref="WinHostFeature"/> that can be configured
///     through a <typeparamref name="TOptions"/> object.
/// </summary>
/// <typeparam name="TOptions">
///     Type of the object used to configure the feature.
/// </typeparam>
public abstract class WinHostFeature<TOptions> : WinHostFeature {
    /// <summary>
    ///     Gets the composition of every delegate passed to
    ///     <see cref="Configure"/>, or <see langword="null"/> when it was
    ///     never called.
    /// </summary>
    /// <remarks>
    ///     Features must apply their own defaults before invoking it, so the
    ///     user configuration always extends the defaults.
    /// </remarks>
    protected Action<TOptions>? ConfigureAction { get; private set; }

    /// <summary>
    ///     Adds a configuration step to the feature.
    /// </summary>
    /// <remarks>
    ///     Every call adds to the previous ones; steps run in call order.
    /// </remarks>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current feature so other actions can be chained.
    /// </returns>
    public WinHostFeature<TOptions> Configure(Action<TOptions> configure) {
        ConfigureAction += Throws.When.Null(configure);

        return this;
    }
}
