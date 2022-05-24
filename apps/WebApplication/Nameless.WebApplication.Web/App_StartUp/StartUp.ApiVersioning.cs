using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void ConfigureApiVersioning(IServiceCollection services) {
            services
                .AddApiVersioning(opts => {
                    opts.AssumeDefaultVersionWhenUnspecified = true;
                    opts.ApiVersionReader = new MediaTypeApiVersionReader();
                    opts.ApiVersionSelector = new CurrentImplementationApiVersionSelector(opts);
                    opts.DefaultApiVersion = new ApiVersion(majorVersion: 1, minorVersion: 0);
                });
        }

        #endregion
    }
}
