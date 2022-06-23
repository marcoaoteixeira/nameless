namespace Nameless.WebApplication.Web {

    public partial class StartUp {

		#region Public Methods

		public void UseSwagger(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseSwagger();
				app.UseSwaggerUI();
			}
		}

		public void ConfigureSwagger(IServiceCollection services) {
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}

		#endregion
	}
}
