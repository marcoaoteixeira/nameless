using Nameless.Localization;
using MS_IHtmlLocalizer = Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizer;
using MS_IHtmlLocalizerFactory = Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizerFactory;

namespace Nameless.AspNetCore.Localization {

    public sealed class HtmlLocalizerFactoryWrapper : MS_IHtmlLocalizerFactory {

        #region Private Read-Only Fields

        private readonly IStringLocalizerFactory _factory;

        #endregion

        #region Public Constructors

        public HtmlLocalizerFactoryWrapper(IStringLocalizerFactory factory) {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        #endregion

        #region MS_IHtmlLocalizerFactory Members

        public MS_IHtmlLocalizer Create(Type resourceSource) => new HtmlLocalizerWrapper(_factory.Create(resourceSource));

        // Need to change baseName and location because Microsoft localization extension uses the base name as the
        // type full name and the location as the assembly name.
        public MS_IHtmlLocalizer Create(string baseName, string location) => new HtmlLocalizerWrapper(_factory.Create(location, baseName));

        #endregion
    }
}
