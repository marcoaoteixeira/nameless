using Microsoft.OpenApi.Models;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

		#region Public Methods

		public void UseSwagger(IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment()) {
				app.UseSwagger();
				app.UseSwaggerUI(_ => _.SwaggerEndpoint("/swagger/v1/swagger.json", $"{env.ApplicationName} v1"));
			}
		}

		public void ConfigureSwagger(IServiceCollection services) {
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen(_ => {
				_.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication", Version = "v1" });
			});
		}

		#endregion
	}
}
