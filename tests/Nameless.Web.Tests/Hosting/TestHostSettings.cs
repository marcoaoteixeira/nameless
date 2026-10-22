namespace Nameless.Web.Hosting;

internal static class TestHostSettings
{
    // Command-line settings for the configuration sections the built-in
    // features require, so a test host can be built without appsettings.
    internal static readonly string[] Required =
    [
        "--DataProtection:UseFileSystem=false",
        "--HealthCheck:LivenessPath=health"
    ];
}
