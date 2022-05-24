using MS_IHtmlLocalizer = Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer;
using MS_LocalizedHtmlString = Microsoft.AspNetCore.Mvc.Localization.LocalizedHtmlString;

namespace Nameless.AspNetCore.Localization {

    public static class HtmlLocalizerExtension {

        #region Public Static Methods

        public static MS_LocalizedHtmlString? Get(this MS_IHtmlLocalizer self, string name, int count = -1, params object[] arguments) {
            if (self == null) { return default; }

            return self is HtmlLocalizerWrapper wrapper
                ? wrapper.Get(name, count, arguments)
                : self[name, arguments];
        }

        #endregion
    }
}
