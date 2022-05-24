namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void ConfigureWebApi(IServiceCollection services) {
            services.AddControllers();
        }

        #endregion
    }
}
