using Autofac.Extensions.DependencyInjection;
using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web {

    public static class EntryPoint {

        #region Public Static Methods

        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(builder => {
                    builder
                        .ConfigureAppConfiguration((ctx, config) => {
                            config.AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true);
                            config.AddJsonFile($"AppSettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true);
                            config.AddEnvironmentVariables();
                        })
                        .UseStartup<StartUp>();
                });

        #endregion
    }
}