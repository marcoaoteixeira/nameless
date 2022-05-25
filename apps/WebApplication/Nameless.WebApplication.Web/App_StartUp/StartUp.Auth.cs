using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Nameless.WebApplication.Web.Security;

namespace Nameless.WebApplication.Web {

    public partial class StartUp {

        #region Public Methods

        public void UseAuth(IApplicationBuilder app) {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        public void ConfigureAuth(IServiceCollection services) {
            var options = GetConfigurationFor<JwtOptions>(Configuration);

            services
                .AddAuthentication(_ => {
                    _.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    _.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(_ => {
                    _.RequireHttpsMetadata = false;
                    _.SaveToken = true;
                    _.TokenValidationParameters = new() {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Secret!)),

                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });
        }

        #endregion
    }
}
