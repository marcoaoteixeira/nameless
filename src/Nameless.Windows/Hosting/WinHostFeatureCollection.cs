using System.Collections;
using Nameless.Windows.Hosting.Features;

namespace Nameless.Windows.Hosting;

/// <summary>
///     Ordered collection of <see cref="WinHostFeature"/> used by
///     <see cref="WinHostFactory"/> to build the Windows host.
/// </summary>
/// <remarks>
///     <para>
///         The collection order is the order in which features are registered
///         and, therefore, the precedence of their service registrations (the
///         last registration of a service wins). Placement is resolved when a feature is inserted; the
///         collection is never sorted.
///     </para>
///     <para>
///         Every lookup by type (<see cref="AddBefore{TAnchor}"/>,
///         <see cref="AddAfter{TAnchor}"/>, <see cref="Replace{TFeature}"/>,
///         <see cref="Remove{TFeature}"/> and <see cref="Get{TFeature}"/>)
///         matches features that are instances of the given type, so a
///         subclass that replaced a built-in feature still acts as that
///         feature. A lookup throws <see cref="InvalidOperationException"/>
///         when nothing or more than one feature matches.
///     </para>
///     <para>
///         A feature type can be added only once, and a feature can't be
///         added next to its base type or one of its subclasses; use
///         <see cref="Replace{TFeature}"/> to swap a feature for a subclass.
///         Sibling subclasses of a shared base type can coexist, so a lookup
///         by that base type can still be ambiguous.
///     </para>
/// </remarks>
public sealed class WinHostFeatureCollection : IReadOnlyCollection<WinHostFeature> {
    private const string FEATURE_NOT_FOUND_MESSAGE = "Feature '{0}' is not registered.";
    private const string FEATURE_AMBIGUOUS_MESSAGE = "Feature '{0}' is ambiguous, it matches: {1}.";
    private const string FEATURE_DUPLICATED_MESSAGE = "Feature '{0}' is already registered.";
    private const string FEATURE_RELATED_MESSAGE = "Feature '{0}' is related by inheritance to the registered feature '{1}'. Use Replace<{1}>() to swap it.";

    private readonly List<WinHostFeature> _features = [];

    /// <inheritdoc />
    public int Count => _features.Count;

    /// <summary>
    ///     Creates a new collection with the features of a console
    ///     application: the ones that do not depend on the user interface.
    /// </summary>
    /// <remarks>
    ///     Registration order matters only when two features register the
    ///     same service: the last one wins.
    /// </remarks>
    /// <returns>
    ///     A new <see cref="WinHostFeatureCollection"/> instance.
    /// </returns>
    public static WinHostFeatureCollection CreateConsoleBase() {
        return new WinHostFeatureCollection()
            .Add(new CommonFeature())
            .Add(new AppConfigurationManagerFeature())
            .Add(new BootstrapFeature())
            .Add(new CompressorFeature())
            .Add(new FileProviderFeature())
            .Add(new GitHubHttpClientFeature())
            .Add(new HttpClientDefaultsFeature())
            .Add(new LocalizationFeature())
            .Add(new LoggingFeature())
            .Add(new LuceneFeature())
            .Add(new MediatorFeature())
            .Add(new ResilienceFeature())
            .Add(new ValidationFeature());
    }

    /// <summary>
    ///     Creates a new collection with every built-in feature, for a
    ///     Windows (WPF) application.
    /// </summary>
    /// <remarks>
    ///     Registration order matters only when two features register the
    ///     same service: the last one wins. The application must provide
    ///     the window types some features depend on, like
    ///     <c>INavigationWindow</c>.
    /// </remarks>
    /// <returns>
    ///     A new <see cref="WinHostFeatureCollection"/> instance.
    /// </returns>
    public static WinHostFeatureCollection CreateWinAppBase() {
        return new WinHostFeatureCollection()
            .Add(new CommonFeature())
            .Add(new AppConfigurationManagerFeature())
            .Add(new BootstrapFeature())
            .Add(new CompressorFeature())
            .Add(new ContentDialogServiceFeature())
            .Add(new DocumentServicesFeature())
            .Add(new FileProviderFeature())
            .Add(new FileSystemDialogFeature())
            .Add(new GitHubHttpClientFeature())
            .Add(new HttpClientDefaultsFeature())
            .Add(new LocalizationFeature())
            .Add(new LoggingFeature())
            .Add(new LuceneFeature())
            .Add(new MediatorFeature())
            .Add(new MessageDialogFeature())
            .Add(new MessengerFeature())
            .Add(new NavigationFeature())
            .Add(new OfficeFeature())
            .Add(new ResilienceFeature())
            .Add(new SnackBarFeature())
            .Add(new StatusReportingFeature())
            .Add(new TaskRunnerFeature())
            .Add(new ValidationFeature())
            .Add(new ViewModelsFeature())
            .Add(new WindowFactoryFeature());
    }

    /// <summary>
    ///     Adds a feature at the end of the collection.
    /// </summary>
    /// <remarks>
    ///     Use <see cref="AddBefore{TAnchor}"/> or
    ///     <see cref="AddAfter{TAnchor}"/> to place it elsewhere.
    /// </remarks>
    /// <param name="feature">
    ///     The feature to add.
    /// </param>
    /// <returns>
    ///     The current collection so other actions can be chained.
    /// </returns>
    public WinHostFeatureCollection Add(WinHostFeature feature) {
        Throws.When.Null(feature);

        return Insert(_features.Count, feature);
    }

    /// <summary>
    ///     Adds a feature right before the <typeparamref name="TAnchor"/>
    ///     feature.
    /// </summary>
    /// <typeparam name="TAnchor">
    ///     Type of the anchor feature.
    /// </typeparam>
    /// <param name="feature">
    ///     The feature to add.
    /// </param>
    /// <returns>
    ///     The current collection so other actions can be chained.
    /// </returns>
    public WinHostFeatureCollection AddBefore<TAnchor>(WinHostFeature feature)
        where TAnchor : WinHostFeature {
        Throws.When.Null(feature);

        var index = GetFeatureIndex<TAnchor>();

        return Insert(index, feature);
    }

    /// <summary>
    ///     Adds a feature right after the <typeparamref name="TAnchor"/>
    ///     feature.
    /// </summary>
    /// <typeparam name="TAnchor">
    ///     Type of the anchor feature.
    /// </typeparam>
    /// <param name="feature">
    ///     The feature to add.
    /// </param>
    /// <returns>
    ///     The current collection so other actions can be chained.
    /// </returns>
    public WinHostFeatureCollection AddAfter<TAnchor>(WinHostFeature feature)
        where TAnchor : WinHostFeature {
        Throws.When.Null(feature);

        var index = GetFeatureIndex<TAnchor>() + 1;

        return Insert(index, feature);
    }

    /// <summary>
    ///     Replaces the <typeparamref name="TFeature"/> feature, keeping its
    ///     position.
    /// </summary>
    /// <remarks>
    ///     The replaced instance is discarded, including anything configured
    ///     on it through <see cref="WinHostFeature{TOptions}.Configure"/>.
    ///     Configure the replacement after replacing.
    /// </remarks>
    /// <typeparam name="TFeature">
    ///     Type of the feature to replace.
    /// </typeparam>
    /// <param name="feature">
    ///     The replacement feature.
    /// </param>
    /// <returns>
    ///     The current collection so other actions can be chained.
    /// </returns>
    public WinHostFeatureCollection Replace<TFeature>(WinHostFeature feature)
        where TFeature : WinHostFeature {
        Throws.When.Null(feature);

        var index = GetFeatureIndex<TFeature>();

        EnsureNotRegistered(feature, ignoreIndex: index);

        _features[index] = feature;

        return this;
    }

    /// <summary>
    ///     Removes the <typeparamref name="TFeature"/> feature.
    /// </summary>
    /// <typeparam name="TFeature">
    ///     Type of the feature to remove.
    /// </typeparam>
    /// <returns>
    ///     The current collection so other actions can be chained.
    /// </returns>
    public WinHostFeatureCollection Remove<TFeature>()
        where TFeature : WinHostFeature {
        var index = GetFeatureIndex<TFeature>();

        _features.RemoveAt(index);

        return this;
    }

    /// <summary>
    ///     Gets the <typeparamref name="TFeature"/> feature.
    /// </summary>
    /// <typeparam name="TFeature">
    ///     Type of the feature.
    /// </typeparam>
    /// <returns>
    ///     The <typeparamref name="TFeature"/> feature instance.
    /// </returns>
    public TFeature Get<TFeature>()
        where TFeature : WinHostFeature {
        var index = GetFeatureIndex<TFeature>();

        return (TFeature)_features[index];
    }

    /// <inheritdoc />
    public IEnumerator<WinHostFeature> GetEnumerator() {
        return _features.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    private WinHostFeatureCollection Insert(int index, WinHostFeature feature) {
        EnsureNotRegistered(feature, ignoreIndex: -1);

        _features.Insert(index, feature);

        return this;
    }

    private int GetFeatureIndex<TFeature>() {
        var type = typeof(TFeature);
        var indexes = Enumerable
            .Range(start: 0, _features.Count)
            .Where(index => type.IsInstanceOfType(_features[index]))
            .ToArray();

        return indexes.Length switch {
            1 => indexes[0],
            0 => throw new InvalidOperationException(string.Format(
                FEATURE_NOT_FOUND_MESSAGE,
                type.GetPrettyName()
            )),
            _ => throw new InvalidOperationException(string.Format(
                FEATURE_AMBIGUOUS_MESSAGE,
                type.GetPrettyName(),
                string.Join(", ", indexes.Select(index => _features[index].GetType().GetPrettyName()))
            ))
        };
    }

    private void EnsureNotRegistered(WinHostFeature feature, int ignoreIndex) {
        var type = feature.GetType();

        for (var index = 0; index < _features.Count; index++) {
            var registered = _features[index].GetType();

            if (index == ignoreIndex || !AreRelated(type, registered)) {
                continue;
            }

            throw new InvalidOperationException(type == registered
                ? string.Format(FEATURE_DUPLICATED_MESSAGE, type.GetPrettyName())
                : string.Format(FEATURE_RELATED_MESSAGE, type.GetPrettyName(), registered.GetPrettyName())
            );
        }
    }

    // A base and its subclass would both match the base type in every
    // lookup, making it ambiguous; siblings share no lookup type besides
    // their common base.
    private static bool AreRelated(Type type, Type other) {
        return type.IsAssignableFrom(other) || other.IsAssignableFrom(type);
    }
}
