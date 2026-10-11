using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;

namespace Nameless.Windows.BackupRecovery;

public static class ServiceCollectionExtensions {
    extension(IServiceCollection self) {
        public IServiceCollection RegisterDisasterRecovery(Action<DisasterRecoveryRegistration> registration) {
            var settings = ActionHelper.FromDelegate(registration);

            var service = typeof(IBackupRecoveryRoutine);
            var implementations = settings.UseAssemblyScan
                ? settings.ExecuteAssemblyScan(service)
                : settings.DisasterRecoveryRoutines;

            var descriptors = implementations.Select(
                implementation => ServiceDescriptor.Transient(service, implementation)
            );

            self.TryAddEnumerable(descriptors);

            return self;
        }
    }
}