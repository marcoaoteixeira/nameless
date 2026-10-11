using Microsoft.AspNetCore.Http;
using Scalar.AspNetCore;

namespace Nameless.Web.Scalar;

/// <summary>
///     Registration settings for Scalar.
/// </summary>
public class ScalarRegistration {
    /// <summary>
    ///     Gets or sets the action used to configure scalar options
    ///     for HTTP context processing.
    /// </summary>
    public Action<ScalarOptions, HttpContext>? ConfigureScalarOptions { get; private set; }

    /// <summary>
    ///     Sets the Scalar configuration action.
    /// </summary>
    /// <param name="configure">
    ///     The configuration delegate.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="ScalarRegistration"/> so other
    ///     actions can be chained.
    /// </returns>
    public ScalarRegistration WithScalarOptionsConfiguration(Action<ScalarOptions, HttpContext> configure) {
        ConfigureScalarOptions = configure;

        return this;
    }
}