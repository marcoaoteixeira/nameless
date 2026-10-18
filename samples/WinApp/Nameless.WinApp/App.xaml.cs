using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nameless.Application;
using Nameless.EntityFrameworkCore;
using Nameless.WinApp.Data;
using Nameless.WinApp.DisasterRecovery;
using Nameless.WinApp.Views.Windows;
using Nameless.Windows;
using Nameless.Windows.Hosting;
using Nameless.Windows.Hosting.Wrappers;
using Nameless.Windows.Localization;
using Wpf.Ui;

namespace Nameless.WinApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
    private static readonly string[] Args = [
        $"--applicationName={Constants.Application.Name}"
    ];

    private static readonly WinHost CurrentHost = WinHostFactory.Create(configure => {
        configure.Args = Args;
        configure.Assemblies = [
            typeof(App).Assembly,
                typeof(AssemblyMarker).Assembly,
                typeof(AssemblyMarkerWindows).Assembly
        ];

        configure.ConfigureAdditionalServices = ConfigureAdditionalServices;
        configure.ConfigureLocalization = ConfigureLocalization;
    });

    public App() {
        ExceptionWarden.Initialize(Constants.Application.Name);
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    protected override async void OnStartup(StartupEventArgs e) {
        CurrentHost.OnStart += ShowMainWindow;

        await CurrentHost.RunAsync();

        base.OnStartup(e);
    }

    // ReSharper disable once AsyncVoidEventHandlerMethod
    protected override async void OnExit(ExitEventArgs e) {
        await CurrentHost.StopAsync();

        CurrentHost.Dispose();

        base.OnExit(e);
    }

    private static Task ShowMainWindow(IServiceProvider provider, CancellationToken cancellationToken) {
        // resolve translation
        L10NExtension.SetLocalizer(
            provider.GetService<ILocalizer>() ?? NullLocalizer.Instance
        );
        
        var main = provider.GetRequiredService<INavigationWindow>();

        provider.GetRequiredService<ISplashScreenWindow>()
                .Show(WindowStartupLocation.CenterScreen);

        main.ShowWindow();

        return Task.CompletedTask;
    }

    private static void ConfigureAdditionalServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment) {
        services.RegisterEntityFrameworkCore<AppDbContext>(registration => {
            registration.WithDbContextConfiguration((provider, ctx) => {
                var applicationContext = provider.GetRequiredService<IApplicationContext>();
                var databaseFile = applicationContext.ApplicationDataFileProvider.GetFile(
                    relativePath: Path.Combine(
                        applicationContext.ApplicationDataFileProvider.GetDatabaseDirectory().Path,
                        SqliteConstants.DatabaseFileName
                    )
                );
                var connStr = string.Format(SqliteConstants.ConnStrPattern, databaseFile.Path);

                ctx.UseSqlite(connStr);
            });
        }, configuration);
    }

    private static void ConfigureLocalization(LocalizationRegistration localization) {
        localization.RegisterLocalizer(typeof(ResourceLocalizer));
    }
}