using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Nameless.Helpers;
using Nameless.Windows.Hosting.Configs;
using Nameless.Windows.Hosting.Internals;
using Nameless.Windows.Hosting.Wrappers;

namespace Nameless.Windows.Hosting;

public sealed class WinHostFactory {
    private WinHostSettings Settings { get; }

    private WinHostFactory(WinHostSettings settings) {
        Settings = settings;
    }

    public static WinHost Create(Action<WinHostSettings>? configure = null) {
        return new WinHostFactory(ActionHelper.FromDelegate(configure)).CreateBuilder()
                                                                       .Build()
                                                                       .Wrap();
    }

    private WinHostBuilder CreateBuilder() {
        var env = string.IsNullOrWhiteSpace(Settings.Environment)
            ? "Development"
            : Settings.Environment;

        return Host.CreateDefaultBuilder(Settings.Args)
                   .ConfigureHostConfiguration(config => {
                       config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                             .AddJsonFile(path: "AppSettings.json", optional: true, reloadOnChange: true)
                             .AddJsonFile(path: $"AppSettings.{env}.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables();
                   })

                   .Wrap()

                   .ConfigureAppConfigurationManagerFeature(Settings)
                   .ConfigureBootstrapFeature(Settings)
                   .ConfigureCommonFeature()
                   .ConfigureZipCompressorFeature(Settings)
                   .ConfigureContentDialogFeature(Settings)
                   .ConfigureDocumentFeature(Settings)
                   .ConfigureFileExplorerFeature(Settings)
                   .ConfigureFileSystemDialogFeature(Settings)
                   .ConfigureGitHubFeature(Settings)
                   .ConfigureHttpClientDefaults(Settings)
                   .ConfigureLocalizationFeature(Settings)
                   .ConfigureLoggingFeature(Settings)
                   .ConfigureLucene(Settings)
                   .ConfigureMediator(Settings)
                   .ConfigureMessageDialog(Settings)
                   .ConfigureMessenger(Settings)
                   .ConfigureNavigation(Settings)
                   .ConfigureOfficeServices(Settings)
                   .ConfigureResilience(Settings)
                   .ConfigureSnackBar(Settings)
                   .ConfigureStatusReporting(Settings)
                   .ConfigureTaskRunner(Settings)
                   .ConfigureValidator(Settings)
                   .ConfigureViewModels(Settings)
                   .ConfigureWindowFactory(Settings)

                   // Intentionally placed last so it can override
                   // other services.
                   .ConfigureAdditionalServices(Settings);
    }
}