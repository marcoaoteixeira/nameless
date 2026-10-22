using Microsoft.AspNetCore.Builder;

namespace Nameless.Web.Hosting;

/// <summary>
///     Represents a feature of the web host, like logging, authentication
///     or CORS, that <see cref="WebHostFactory"/> registers and uses when
///     building the web application.
/// </summary>
/// <remarks>
///     Features are kept in a <see cref="WebHostFeatureCollection"/>,
///     configured through <see cref="WebHostFactory.Create"/>. The
///     factory calls <see cref="Register"/> on every feature, in collection
///     order, before building the <see cref="WebApplication"/>, and then
///     <see cref="Use"/> on every feature, in the same order, after building
///     it. Since middlewares run in the order they're added, the position of
///     a feature in the collection defines its position in the request
///     pipeline.
/// </remarks>
public abstract class WebHostFeature {
    /// <summary>
    ///     Gets the feature name.
    /// </summary>
    public virtual string Name => GetType().Name;
    
    /// <summary>
    ///     Registers the services required by the feature.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="WebApplicationBuilder"/> being configured.
    /// </param>
    /// <param name="context">
    ///     The <see cref="WebHostFeatureContext"/> shared by every feature.
    /// </param>
    public virtual void Register(WebApplicationBuilder builder, WebHostFeatureContext context) { }

    /// <summary>
    ///     Configures the middlewares or endpoints required by the feature.
    /// </summary>
    /// <param name="app">
    ///     The built <see cref="WebApplication"/>.
    /// </param>
    /// <param name="context">
    ///     The <see cref="WebHostFeatureContext"/> shared by every feature.
    /// </param>
    public virtual void Use(WebApplication app, WebHostFeatureContext context) { }
}

/// <summary>
///     Represents a <see cref="WebHostFeature"/> that can be configured
///     through a <typeparamref name="TOptions"/> object.
/// </summary>
/// <typeparam name="TOptions">
///     Type of the object used to configure the feature.
/// </typeparam>
public abstract class WebHostFeature<TOptions> : WebHostFeature {
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
    public WebHostFeature<TOptions> Configure(Action<TOptions> configure) {
        ConfigureAction += Throws.When.Null(configure);

        return this;
    }
}
