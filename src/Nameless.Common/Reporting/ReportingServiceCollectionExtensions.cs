using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Nameless.Reporting;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods that register
///     the <see cref="IStatusReporter{TService}"/> services.
/// </summary>
public static class ReportingServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/> instance.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers Status Reporting feature.
        /// </summary>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <returns>
        ///     The current instance of <see cref="IServiceCollection"/> so other
        ///     actions can be chained.
        /// </returns>
        public IServiceCollection RegisterStatusReporting(IConfiguration configuration) {
            self.ConfigureOptions<StatusReportingOptions>(configuration);

            self.TryAddSingleton<IStatusReportingHub, StatusReportingHub>();
            self.TryAddTransient(typeof(IStatusReporter<>), typeof(StatusReporter<>));
            self.TryAddTransient(typeof(IStatusMonitor<>), typeof(StatusMonitor<>));

            return self;
        }
    }
}