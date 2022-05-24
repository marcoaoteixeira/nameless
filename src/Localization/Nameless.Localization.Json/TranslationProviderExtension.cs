using System.Globalization;
using Nameless.Helpers;
using Nameless.Localization.Json.Schema;

namespace Nameless.Localization.Json {

    public static class TranslationProviderExtension {

        #region Public Static Methods

        public static TranslationGroup? Get(this ITranslationProvider self, CultureInfo? culture) {
            if (self == null) { return default; }

            return AsyncHelper.RunSync(() => self.GetAsync(culture));
        }

        #endregion
    }
}
