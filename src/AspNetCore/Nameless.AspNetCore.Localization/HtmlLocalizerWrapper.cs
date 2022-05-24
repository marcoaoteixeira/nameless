using Nameless.Localization;
using MS_IHtmlLocalizer = Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer;
using MS_LocalizedHtmlString = Microsoft.AspNetCore.Mvc.Localization.LocalizedHtmlString;
using MS_LocalizedString = Microsoft.Extensions.Localization.LocalizedString;


namespace Nameless.AspNetCore.Localization {

    public sealed class HtmlLocalizerWrapper : MS_IHtmlLocalizer {

        #region Private Read-Only Fields

        private readonly IStringLocalizer _localizer;

        #endregion

        #region Public Constructors

        public HtmlLocalizerWrapper(IStringLocalizer localizer) {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        #endregion

        #region Public Methods

        public MS_LocalizedHtmlString? Get(string name, int count = -1, params object[] arguments) {
            return _localizer[name, count, arguments].ToLocalizedHtmlString();
        }

        #endregion

        #region MS_IHtmlLocalizer Members

        public MS_LocalizedHtmlString this[string name] {
            get {
                var result = _localizer[name].ToLocalizedHtmlString();
                return result ?? new MS_LocalizedHtmlString(name, name);
            }
        }

        public MS_LocalizedHtmlString this[string name, params object[] arguments] {
            get {
                var result = _localizer[name, args: arguments].ToLocalizedHtmlString();
                return result ?? new MS_LocalizedHtmlString(name, name, isResourceNotFound: false, arguments: arguments);
            }
        }

        public IEnumerable<MS_LocalizedString> GetAllStrings(bool includeParentCultures) {
            foreach (var item in _localizer.List(includeParentCultures)) {
                var result = item.ToLocalizedString();
                if (result != null) {
                    yield return result;
                }
            }
        }

        public MS_LocalizedString GetString(string name) {
            var result = _localizer[name].ToLocalizedString();
            return result ?? new MS_LocalizedString(name, name);
        }

        public MS_LocalizedString GetString(string name, params object[] arguments) {
            var result = _localizer[name, args: arguments].ToLocalizedString();
            return result ?? new MS_LocalizedString(name, string.Format(name, arguments));
        }

        #endregion

    }
}
