namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void UseCors(IApplicationBuilder app) {
            app.UseCors(policy => {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        }

        public void ConfigureCors(IServiceCollection services) {
            // CORS defines a way in which a browser and server can
            // interact to determine whether or not it is safe to
            // allow the cross-origin request.
            services.AddCors();
        }

        #endregion
    }
}
