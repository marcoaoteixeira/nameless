using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Nameless.Configuration;
using Nameless.Helpers;

namespace Nameless.Web.Auth;

public static class AuthServiceCollectionExtensions {
    extension(IServiceCollection self) {
        public IServiceCollection RegisterAuth(Action<AuthRegistration>? configure = null, IConfiguration? configuration = null) {
            var registration = ActionHelper.FromDelegate(configure);

            return self.AddAuthorization(registration.ConfigureAuthorization ?? (_ => { }))
                       .AddAuthentication(registration, configuration);
        }

        private IServiceCollection AddAuthentication(AuthRegistration registration, IConfiguration? configuration) {
            var wrapper = new AuthenticationBuilderWrapper(self);
            var configure = registration.ConfigureAuthentication ?? (builder => ConfigureAuthenticationDefaults(builder, configuration));

            configure(wrapper);

            wrapper.Apply();

            return self;
        }
    }

    private static void ConfigureAuthenticationDefaults(IAuthenticationBuilder builder, IConfiguration? configuration) {
        builder.Configure(opts => opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(opts => {
                   var section = configuration?
                                 .GetSection<JsonWebTokenOptions>()
                                 .GetChildren()
                                 .FirstOrDefault();

                   if (section is null) { return; }

                   var jwt = section.GetOrThrow<JsonWebTokenOptions>();

                   opts.Authority = jwt.Authority;
                   opts.TokenValidationParameters = new TokenValidationParameters {
                       ValidIssuers = jwt.Issuers,
                       ValidateIssuer = jwt.ValidateIssuer,

                       ValidAudiences = jwt.Audiences,
                       ValidateAudience = jwt.ValidateAudience,

                       ValidateLifetime = jwt.ValidateLifetime,

                       ClockSkew = jwt.ClockSkew
                   };
               });
    }
}