using Nameless.EntityFrameworkCore;
using Nameless.Microservices.Api.AutoGenCode;
using Nameless.Microservices.Api.Data;
using Nameless.Web;
using Nameless.Web.Hosting;
using Nameless.Web.Scalar;

namespace Nameless.Microservices.Api;

public class EntryPoint {
    public static void Main(string[] args) {
        WebHostFactory.Create(settings => {
            settings.Args = args;
            settings.Assemblies = [
                typeof(EntryPoint).Assembly,
                typeof(AssemblyMarker).Assembly
            ];

            settings.ConfigureAdditionalServices = ConfigureAdditionalServices;
            settings.ExecuteBeforeStartup = ExecuteBeforeStartup;
            settings.ConfigureScalar = ConfigureScalar;

            DisableServices(settings);
        }).Run();
    }

    private static void ConfigureAdditionalServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment) {
        services.RegisterEntityFrameworkCore<AppDbContext>();
        services.RegisterOAuthAuthenticationTokenProvider(configuration);
        services.RegisterAutoEndpoints();
    }

    private static void ExecuteBeforeStartup(IApplicationBuilder app) {
        app.EndpointRouteBuilder.MapAutoEndpoints();
    }

    private static void ConfigureScalar(ScalarRegistration scalar) {
        scalar.UseDefaultHttpAuthentication = true;
    }

    private static void DisableServices(WebHostSettings settings) {
        settings.DisableBootstrap = true;
    }
}