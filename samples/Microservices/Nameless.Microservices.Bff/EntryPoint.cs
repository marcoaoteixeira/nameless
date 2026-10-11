using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Microservices.Bff.AutoGenCode;
using Nameless.Microservices.Bff.Domains.Chores.External;
using Nameless.Web;
using Nameless.Web.Auth;
using Nameless.Web.Hosting;
using Nameless.Web.Scalar;

namespace Nameless.Microservices.Bff;

/// <summary>
///     BFF entry point
/// </summary>
public class EntryPoint
{
    /// <summary>
    ///     BFF entry point method.
    /// </summary>
    /// <param name="args">
    ///     The arguments.
    /// </param>
    public static void Main(string[] args)
    {
        WebHostFactory.Create(settings =>
        {
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

    private static void ConfigureAdditionalServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.RegisterChoresHttpClient();
        services.RegisterOAuthAuthenticationTokenProvider(configuration);
        services.RegisterAutoEndpoints();
        services.TryAddTransient<AuthorizationForwardingHandler>();
    }

    private static void ExecuteBeforeStartup(IApplicationBuilder app)
    {
        app.EndpointRouteBuilder.MapAutoEndpoints();
    }

    private static void ConfigureScalar(ScalarRegistration scalar)
    {
        scalar.UseDefaultHttpAuthentication = true;
    }

    private static void DisableServices(WebHostSettings settings)
    {
        settings.DisableAntiforgery = true;
        settings.DisableDataProtection = true;
        settings.DisableMediator = true;
        settings.DisableOutputCache = true;
        settings.DisableRequestTimeouts = true;
        settings.DisablePeriodicWorkers = true;
    }
}
