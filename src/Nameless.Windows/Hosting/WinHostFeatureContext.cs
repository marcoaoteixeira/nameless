using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Nameless.Windows.Hosting;

/// <summary>
///     Read-only data shared by every <see cref="WinHostFeature"/> while the
///     host is being built.
/// </summary>
public sealed class WinHostFeatureContext {
    /// <summary>
    ///     Gets the assemblies to scan for types, resolved by
    ///     <see cref="WinHostFactory.CreateWinApp"/>: the library assemblies first,
    ///     then the given assemblies or, when none were given, the calling
    ///     one.
    /// </summary>
    public IReadOnlyList<Assembly> Assemblies { get; }

    /// <summary>
    ///     Gets the startup logger.
    /// </summary>
    /// <remarks>
    ///     Every startup log entry uses the same category, the full name of
    ///     <see cref="WinHostFactory"/>. The logger is disposed once the host
    ///     is built, so use it only inside
    ///     <see cref="WinHostFeature.Register"/>; don't capture it in
    ///     callbacks that run later. Resolve an <c>ILogger&lt;T&gt;</c> from
    ///     the application services instead.
    /// </remarks>
    public ILogger Logger { get; }

    /// <summary>
    ///     Initializes a new instance of <see cref="WinHostFeatureContext"/>.
    /// </summary>
    /// <param name="assemblies">
    ///     The assemblies to scan for types.
    /// </param>
    /// <param name="logger">
    ///     The startup logger.
    /// </param>
    public WinHostFeatureContext(IReadOnlyList<Assembly> assemblies, ILogger logger) {
        // Copy, so a feature cannot change the list seen by the others.
        Assemblies = [.. Throws.When.Null(assemblies)];
        Logger = Throws.When.Null(logger);
    }
}
