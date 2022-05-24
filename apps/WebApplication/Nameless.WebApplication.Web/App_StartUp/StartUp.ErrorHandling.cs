using ElmahCore.Mvc;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void ConfigureErrorHandling(IServiceCollection services) {
            services.AddElmah(opts => {
                opts.OnPermissionCheck = ctx => ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated && ctx.User.IsInRole("SYS_ADMINISTRATOR");
                opts.LogPath = "~/elmah";
            });
        }

        public void UseErrorHandling(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); } else { app.UseElmah(); }
        }

        #endregion
    }
}
