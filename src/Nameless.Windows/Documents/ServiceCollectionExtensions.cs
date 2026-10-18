using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;

namespace Nameless.Windows.Documents;

public static class ServiceCollectionExtensions {
    extension(IServiceCollection self) {
        public IServiceCollection RegisterDocumentServices(Action<DocumentServicesRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.TryAddSingleton<IDocumentService, DocumentService>();
            self.RegisterDocumentReaders(registration);
            self.RegisterDocumentConverters(registration);

            return self;
        }

        private void RegisterDocumentConverters(DocumentServicesRegistration registration) {
            self.TryAddEnumerable(registration.DocumentConverters.Select(
                implementation => ServiceDescriptor.Singleton(
                    typeof(IDocumentConverter),
                    implementation
                )
            ));
        }

        private void RegisterDocumentReaders(DocumentServicesRegistration registration) {
            self.TryAddEnumerable(registration.DocumentReaders.Select(
                implementation => ServiceDescriptor.Singleton(
                    typeof(IDocumentReader),
                    implementation
                )
            ));
        }
    }
}