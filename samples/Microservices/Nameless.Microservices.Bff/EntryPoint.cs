using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Microservices.Bff.AutoGenCode;
using Nameless.Microservices.Bff.Domains.Chores.External;
using Nameless.Web;
using Nameless.Web.Auth;
using Nameless.Web.Hosting;
using Nameless.Web.Hosting.Features;
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
        WebHostFactory.Create(
            args,
            [
                typeof(EntryPoint).Assembly,
                typeof(AssemblyMarker).Assembly
            ],
            ConfigureFeatures
        ).Run();
    }

    private static void ConfigureFeatures(WebHostFeatureCollection features)
    {
        features
            .Remove<AntiforgeryFeature>()
            .Remove<DataProtectionFeature>()
            .Remove<MediatorFeature>()
            .Remove<OutputCacheFeature>()
            .Remove<RequestTimeoutFeature>()
            .Remove<PeriodicWorkersFeature>();

        features.Get<ScalarFeature>().Configure(ConfigureScalar);

        // Added last, so its registrations override the built-in ones.
        features.Add(new ApplicationFeature());
    }

    private sealed class ApplicationFeature : WebHostFeature
    {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context)
        {
            builder.Services.RegisterChoresHttpClient();
            builder.Services.RegisterOAuthAuthenticationTokenProvider(builder.Configuration);
            builder.Services.RegisterAutoEndpoints();
            builder.Services.TryAddTransient<AuthorizationForwardingHandler>();
        }

        public override void Use(WebApplication app, WebHostFeatureContext context)
        {
            app.EndpointRouteBuilder.MapAutoEndpoints();
        }
    }

    private static void ConfigureScalar(ScalarRegistration scalar)
    {
        scalar.UseDefaultHttpAuthentication = true;
    }
}
