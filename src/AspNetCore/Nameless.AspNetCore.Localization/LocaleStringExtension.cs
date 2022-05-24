using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Nameless.Localization;

namespace Nameless.AspNetCore.Localization {

    public static class LocaleStringExtension {

        #region Public Static Methods

        public static LocalizedHtmlString? ToLocalizedHtmlString(this LocaleString self) {
            if (self == null) { return default; }

            return new LocalizedHtmlString(self.Text, self.Translation, isResourceNotFound: false, arguments: self.Args);
        }

        public static LocalizedString? ToLocalizedString(this LocaleString self) {
            if (self == null) { return default; }

            return new LocalizedString(self.GetOriginal(), self.GetTranslation());
        }

        #endregion
    }
}
