using System.Globalization;
using Nameless.Localization.Json.Schema;

namespace Nameless.Localization.Json {

    public sealed class StringLocalizer : IStringLocalizer {

        #region Private Read-Only Fields

        private readonly CultureInfo _culture;
        private readonly string _sourceName;
        private readonly string _sourcePath;
        private readonly PluralizationRuleDelegate _pluralizationRule;
        private readonly TranslationCollection _translationCollection;

        private readonly Func<CultureInfo, string, string, IStringLocalizer> _localizerFactory;

        #endregion

        #region Public Constructors

        public StringLocalizer(CultureInfo culture, string sourceName, string sourcePath, PluralizationRuleDelegate pluralizationRule, TranslationCollection translationCollection, Func<CultureInfo, string, string, IStringLocalizer> localizerFactory) {
            Prevent.Null(culture, nameof(culture));
            Prevent.NullEmptyOrWhiteSpace(sourceName, nameof(sourceName));
            Prevent.NullEmptyOrWhiteSpace(sourcePath, nameof(sourcePath));
            Prevent.Null(pluralizationRule, nameof(pluralizationRule));
            Prevent.Null(translationCollection, nameof(translationCollection));
            Prevent.Null(localizerFactory, nameof(localizerFactory));


            _culture = culture;
            _sourceName = sourceName;
            _sourcePath = sourcePath;
            _pluralizationRule = pluralizationRule;
            _translationCollection = translationCollection;

            _localizerFactory = localizerFactory;
        }

        #endregion

        #region IStringLocalizer Members

        public LocaleString this[string text, int count = -1, params object[] args] {
            get {
                if (!_translationCollection.TryGetValue(text, out var translation)) {
                    return new LocaleString(_culture, text, null, args);
                }

                var pluralForm = _pluralizationRule(count);
                if (pluralForm >= translation!.Values.Length) {
                    throw new PluralFormNotFoundException($"Couldn't locate plural form '{pluralForm}' message '{text}'.");
                }

                return new LocaleString(_culture, translation.Key, translation.Values[pluralForm], args);
            }
        }

        public IEnumerable<LocaleString> List(bool includeParentCultures = false) {
            foreach (var translation in _translationCollection.Values) {
                foreach (var value in translation.Values) {
                    yield return new LocaleString(_culture, value, value);
                }
            }

            if (includeParentCultures) {
                foreach (var culture in _culture.GetTree().Skip(1)) {
                    var localizer = _localizerFactory(culture, _sourceName, _sourcePath);
                    foreach (var translation in localizer.List(includeParentCultures: false)) {
                        yield return translation;
                    }
                }
            }
        }

        #endregion
    }
}
