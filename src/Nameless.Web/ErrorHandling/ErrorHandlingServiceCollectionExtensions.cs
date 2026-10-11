using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;

namespace Nameless.Web.ErrorHandling;

public static class ErrorHandlingServiceCollectionExtensions {
    extension(IServiceCollection self) {
        public IServiceCollection RegisterErrorHandling(Action<ErrorHandlingRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);
            
            var descriptions = registration.ExceptionHandlers.Select(
                implementation => ServiceDescriptor.Singleton(typeof(IExceptionHandler), implementation)
            );

            self.TryAdd(descriptions);

            return self;
        }
    }
}
