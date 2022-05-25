using System.Globalization;

namespace Nameless.Localization {

    /// <summary>
    /// Singleton Pattern implementation for <see cref="DefaultPluralizationRuleProvider" />. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    [Singleton]
    public sealed class DefaultPluralizationRuleProvider : IPluralizationRuleProvider {

        #region Private Static Read-Only Fields

        private static readonly IPluralizationRuleProvider _instance = new DefaultPluralizationRuleProvider();
        private static readonly IDictionary<string, PluralizationRuleDelegate> _rules = new Dictionary<string, PluralizationRuleDelegate>();

        #endregion

        #region Public Static Read-Only Fields

        public static readonly PluralizationRuleDelegate DefaultRule = (count) => count >= 1 ? 1 : 0;

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of <see cref="DefaultPluralizationRuleProvider" />.
        /// </summary>
        public static IPluralizationRuleProvider Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static DefaultPluralizationRuleProvider() {
            AddRule(new[] { "ay", "bo", "cgg", "dz", "fa", "id", "ja", "jbo", "ka", "kk", "km", "ko", "ky", "lo", "ms", "my", "sah", "su", "th", "tt", "ug", "vi", "wo", "zh" }, n => 0);
            AddRule(new[] { "ach", "ak", "am", "arn", "br", "fil", "fr", "gun", "ln", "mfe", "mg", "mi", "oc", "pt-BR", "tg", "ti", "tr", "uz", "wa" }, n => (n > 1 ? 1 : 0));
            AddRule(new[] { "af", "an", "anp", "as", "ast", "az", "bg", "bn", "brx", "ca", "da", "de", "doi", "el", "en", "eo", "es", "es-AR", "et", "eu", "ff", "fi", "fo", "fur", "fy", "gl", "gu", "ha", "he", "hi", "hne", "hu", "hy", "ia", "it", "kl", "kn", "ku", "lb", "mai", "ml", "mn", "mni", "mr", "nah", "nap", "nb", "ne", "nl", "nn", "no", "nso", "or", "pa", "pap", "pms", "ps", "pt", "rm", "rw", "sat", "sco", "sd", "se", "si", "so", "son", "sq", "sv", "sw", "ta", "te", "tk", "ur", "yo" }, n => (n != 1 ? 1 : 0));
            AddRule(new[] { "is" }, n => (n % 10 != 1 || n % 100 == 11 ? 1 : 0));
            AddRule(new[] { "jv" }, n => (n != 0 ? 1 : 0));
            AddRule(new[] { "mk" }, n => (n == 1 || n % 10 == 1 ? 0 : 1));
            AddRule(new[] { "be", "bs", "hr", "lt" }, n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
            AddRule(new[] { "cs" }, n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2));
            AddRule(new[] { "csb", "pl" }, n => ((n == 1) ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
            AddRule(new[] { "lv" }, n => (n % 10 == 1 && n % 100 != 11 ? 0 : n != 0 ? 1 : 2));
            AddRule(new[] { "mnk" }, n => (n == 0 ? 0 : n == 1 ? 1 : 2));
            AddRule(new[] { "ro" }, n => (n == 1 ? 0 : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? 1 : 2));
            AddRule(new[] { "cy" }, n => ((n == 1) ? 0 : (n == 2) ? 1 : (n != 8 && n != 11) ? 2 : 3));
            AddRule(new[] { "gd" }, n => ((n == 1 || n == 11) ? 0 : (n == 2 || n == 12) ? 1 : (n > 2 && n < 20) ? 2 : 3));
            AddRule(new[] { "kw" }, n => ((n == 1) ? 0 : (n == 2) ? 1 : (n == 3) ? 2 : 3));
            AddRule(new[] { "mt" }, n => (n == 1 ? 0 : n == 0 || (n % 100 > 1 && n % 100 < 11) ? 1 : (n % 100 > 10 && n % 100 < 20) ? 2 : 3));
            AddRule(new[] { "sl" }, n => (n % 100 == 1 ? 1 : n % 100 == 2 ? 2 : n % 100 == 3 || n % 100 == 4 ? 3 : 0));
            AddRule(new[] { "ru", "sr", "uk" }, n => (n % 10 == 1 && n % 100 != 11 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2));
            AddRule(new[] { "sk" }, n => ((n == 1) ? 0 : (n >= 2 && n <= 4) ? 1 : 2));
            AddRule(new[] { "ga" }, n => (n == 1 ? 0 : n == 2 ? 1 : (n > 2 && n < 7) ? 2 : (n > 6 && n < 11) ? 3 : 4));
            AddRule(new[] { "ar" }, n => (n == 0 ? 0 : n == 1 ? 1 : n == 2 ? 2 : n % 100 >= 3 && n % 100 <= 10 ? 3 : n % 100 >= 11 ? 4 : 5));
        }

        #endregion

        #region Private Constructors

        private DefaultPluralizationRuleProvider() { }

        #endregion

        #region Private Static Methods

        private static void AddRule(string[] cultures, PluralizationRuleDelegate rule) {
            foreach (var culture in cultures) {
                _rules.Add(culture, rule);
            }
        }

        #endregion

        #region IPluralizationRuleProvider Members

        /// <inheritdocs />
        public bool TryGet(CultureInfo culture, out PluralizationRuleDelegate? rule) {
            Prevent.Null(culture, nameof(culture));

            rule = null;

            if (culture.Name == string.Empty) { return false; }

            return _rules.TryGetValue(culture.Name, out rule) || TryGet(culture.Parent, out rule);
        }

        #endregion
    }
}