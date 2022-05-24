using System.Globalization;
using Nameless.Localization.Json.Schema;

namespace Nameless.Localization.Json {

    /// <summary>
    /// Holds all mechanics to create string localizers.
    /// For performance sake, use only one instance of this class
    /// through the application life time.
    /// </summary>
    public sealed class StringLocalizerFactory : IStringLocalizerFactory {

        #region Private Read-Only Fields

        private readonly ICultureContext _cultureContext;
        private readonly IPluralizationRuleProvider _pluralizationRuleProvider;
        private readonly ITranslationProvider _translationProvider;

        #endregion

        #region Public Constructors

        public StringLocalizerFactory(ICultureContext cultureContext, IPluralizationRuleProvider pluralizationRuleProvider, ITranslationProvider translationProvider) {
            Ensure.NotNull(cultureContext, nameof(cultureContext));
            Ensure.NotNull(translationProvider, nameof(translationProvider));
            Ensure.NotNull(pluralizationRuleProvider, nameof(pluralizationRuleProvider));

            _cultureContext = cultureContext;
            _pluralizationRuleProvider = pluralizationRuleProvider;
            _translationProvider = translationProvider;
        }

        #endregion

        #region Private Methods

        private PluralizationRuleDelegate GetPluralizationRuleDelegate(CultureInfo culture) {
            return _pluralizationRuleProvider.TryGet(culture, out var pluralizationRule)
                ? pluralizationRule!
                : DefaultPluralizationRuleProvider.DefaultRule;
        }

        private TranslationCollection GetTranslationCollection(CultureInfo culture, string sourceName, string sourcePath) {
            var key = $"[{sourceName}] {sourcePath}";
            var translationGroup = _translationProvider.Get(culture);
            return translationGroup!.TryGetValue(key, out var translationCollection)
                ? translationCollection!
                : new TranslationCollection(key);
        }

        private StringLocalizer GetLocalizer(CultureInfo culture, string sourceName, string sourcePath) {
            var pluralizationRule = GetPluralizationRuleDelegate(culture);
            var translationCollection = GetTranslationCollection(culture, sourceName, sourcePath);

            return new StringLocalizer(culture, sourceName, sourcePath, pluralizationRule, translationCollection, GetLocalizer);
        }

        #endregion

        #region ILocalizerFactory Members

        /// <inheritdocs />
        /// <remarks>
        /// If we're talking about a resource named Something.Somewhere.Thing (Assembly name)
        /// and a resource string for the Thingable class, so, the <paramref name="sourceName"/>
        /// will be "Something.Somewhere.Thing" and the <paramref name="sourcePath"/> will be
        /// "Something.Somewhere.Thing.Thingable". Also, the key in the translation file will be
        /// "[Something.Somewhere.Thing] Something.Somewhere.Thing.Thingable"
        /// </remarks>
        public IStringLocalizer Create(Type source) => Create(source.Assembly.GetName().Name!, source.FullName!);

        /// <inheritdocs />
        /// <remarks>
        /// If we're talking about a resource named Something.Somewhere.Thing (Assembly name)
        /// and a resource string for the Thingable class, so, the <paramref name="sourceName"/>
        /// will be "Something.Somewhere.Thing" and the <paramref name="sourcePath"/> will be
        /// "Something.Somewhere.Thing.Thingable". Also, the key in the translation file will be
        /// "[Something.Somewhere.Thing] Something.Somewhere.Thing.Thingable"
        /// </remarks>
        public IStringLocalizer Create(string sourceName, string sourcePath) => GetLocalizer(_cultureContext.GetCulture(), sourceName, sourcePath);

        #endregion
    }
}
