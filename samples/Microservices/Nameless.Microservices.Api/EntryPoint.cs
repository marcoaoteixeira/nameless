using Nameless.EntityFrameworkCore;
using Nameless.Microservices.Api.AutoGenCode;
using Nameless.Microservices.Api.Data;
using Nameless.Web;
using Nameless.Web.Hosting;
using Nameless.Web.Hosting.Features;
using Nameless.Web.Scalar;

namespace Nameless.Microservices.Api;

public class EntryPoint {
    public static void Main(string[] args) {
        WebHostFactory.Create(
            args,
            [
                typeof(EntryPoint).Assembly,
                typeof(AssemblyMarker).Assembly
            ],
            ConfigureFeatures
        ).Run();
    }

    private static void ConfigureFeatures(WebHostFeatureCollection features) {
        features.Get<ScalarFeature>().Configure(ConfigureScalar);

        // Added last, so its registrations override the built-in ones.
        features.Add(new ApplicationFeature());
    }

    private sealed class ApplicationFeature : WebHostFeature {
        public override void Register(WebApplicationBuilder builder, WebHostFeatureContext context) {
            builder.Services.RegisterEntityFrameworkCore<AppDbContext>();
            builder.Services.RegisterOAuthAuthenticationTokenProvider(builder.Configuration);
            builder.Services.RegisterAutoEndpoints();
        }

        public override void Use(WebApplication app, WebHostFeatureContext context) {
            app.EndpointRouteBuilder.MapAutoEndpoints();
        }
    }

    private static void ConfigureScalar(ScalarRegistration scalar) {
        scalar.UseDefaultHttpAuthentication = true;
    }
}
