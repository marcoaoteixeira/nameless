using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Backup.Internals;
using Nameless.Helpers;

namespace Nameless.Backup;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class BackupServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the backup &amp; restore services and routines.
        /// </summary>
        /// <param name="configure">
        ///     The registration action.
        /// </param>
        /// <param name="configuration">
        ///     The configuration (section <c>Backup</c>) bound to
        ///     <see cref="BackupOptions"/>.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other
        ///     actions can be chained.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         Requires an <c>ICompressor</c>; register one with
        ///         <c>RegisterZipCompressor</c>.
        ///     </para>
        ///     <para>
        ///         <see cref="IBackupService"/> and the routines are
        ///         transient; resolve the service from a scope when routines
        ///         depend on scoped services. Routine keys are validated when
        ///         the service is resolved.
        ///     </para>
        /// </remarks>
        public IServiceCollection RegisterBackupService(Action<BackupRegistration>? configure = null, IConfiguration? configuration = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.ConfigureOptions<BackupOptions>(configuration);

            self.TryAddSingleton(TimeProvider.System);
            self.TryAddSingleton<BackupOperationGate>();
            self.TryAddTransient<IBackupService, BackupService>();
            self.TryAddEnumerable(
                registration.Routines.Select(
                    routine => ServiceDescriptor.Transient(typeof(IBackupRoutine), routine)
                )
            );

            return self;
        }
    }
}
