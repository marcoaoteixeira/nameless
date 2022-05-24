using Microsoft.Extensions.DependencyInjection;
using MS_IHtmlLocalizerFactory = Microsoft.AspNetCore.Mvc.Localization.IHtmlLocalizerFactory;

namespace Nameless.AspNetCore.Localization {

    public static class ServiceCollectionExtension {

        #region Public Static Methods

        public static IServiceCollection? AddMvcLocalizationWrappers(this IServiceCollection self) {
            if (self == null) { return default; }

            return self.AddSingleton<MS_IHtmlLocalizerFactory, HtmlLocalizerFactoryWrapper>();
        }

        #endregion
    }
}
