namespace Nameless.WebApplication.Web {

    public partial class StartUp {

		#region Public Methods

		public void ConfigureEndpoints(IServiceCollection services) {
			services.AddRouting();
		}

		public void UseEndpoints(IApplicationBuilder app) {
			app.UseRouting();

			app.UseEndpoints(endpoints => {
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "api/v{version:apiVersion}/{controller=Values}/{action=Get}/{id?}"
				);
			});
		}

		#endregion
	}
}
