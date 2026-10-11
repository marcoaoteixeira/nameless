using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nameless.Helpers;
using Nameless.Mediator.Events;
using Nameless.Mediator.Requests;
using Nameless.Mediator.Streams;

namespace Nameless.Mediator;

/// <summary>
///     <see cref="IServiceCollection"/> extension methods.
/// </summary>
public static class MediatorServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Registers the mediator services.
        /// </summary>
        /// <param name="configure">
        ///     The configuration action.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other
        ///     actions can be chained.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     if a handler or pipeline behavior can never be resolved or
        ///     invoked by the mediator, or if a request or stream ends up
        ///     with more than one handler (including handlers already
        ///     present in the service collection).
        /// </exception>
        public IServiceCollection RegisterMediator(Action<MediatorRegistration>? configure = null) {
            var registration = ActionHelper.FromDelegate(configure);

            self.TryAddTransient<IMediator, MediatorImpl>();

            return self.RegisterEvents(registration)
                       .RegisterRequests(registration)
                       .RegisterStreams(registration);
        }

        private IServiceCollection RegisterEvents(MediatorRegistration registration) {
            self.TryAddTransient<IEventHandlerInvoker, EventHandlerInvoker>();
            self.RegisterHandlers(typeof(IEventHandler<>), registration.EventHandlers, multiple: true);

            return self;
        }

        private IServiceCollection RegisterRequests(MediatorRegistration registration) {
            self.TryAddTransient<IRequestHandlerInvoker, RequestHandlerInvoker>();

            // each access to the handlers collection executes the assembly
            // scan again, read it once.
            var handlers = registration.RequestHandlers;
            self.RegisterHandlers(typeof(IRequestHandler<,>), handlers, multiple: false);
            self.RegisterHandlers(typeof(IRequestHandler<>), handlers, multiple: false);

            self.RegisterPipelineBehaviors(typeof(IRequestPipelineBehavior<,>), registration.RequestPipelineBehaviors);
            self.RegisterPipelineBehaviors(typeof(IRequestPipelineBehavior<>), registration.RequestPipelineBehaviors);

            return self;
        }

        private IServiceCollection RegisterStreams(MediatorRegistration registration) {
            self.TryAddTransient<IStreamHandlerInvoker, StreamHandlerInvoker>();
            self.RegisterHandlers(typeof(IStreamHandler<,>), registration.StreamHandlers, multiple: false);

            self.RegisterPipelineBehaviors(typeof(IStreamPipelineBehavior<,>), registration.StreamPipelineBehaviors);

            return self;
        }

        private void RegisterHandlers(Type service, IReadOnlyCollection<Type> implementations, bool multiple) {
            // open generic implementations first, followed by the closed ones.
            var descriptors = CreateDescriptors(service, [.. implementations.OrderBy(implementation => implementation.IsGenericTypeDefinition ? 0 : 1)]);

            if (multiple) {
                self.TryAddEnumerable(descriptors);

                return;
            }

            foreach (var descriptor in descriptors) {
                self.AddSingleHandler(descriptor);
            }
        }

        // A request (or stream) must have exactly one handler. Registering
        // the same implementation again is a no-op, so RegisterMediator stays
        // idempotent; a different implementation for the same service is a
        // configuration error.
        private void AddSingleHandler(ServiceDescriptor descriptor) {
            var current = self.FirstOrDefault(item => !item.IsKeyedService &&
                                                      item.ServiceType == descriptor.ServiceType);

            if (current is null) {
                self.Add(descriptor);

                return;
            }

            if (current.ImplementationType == descriptor.ImplementationType) { return; }

            var currentName = current.ImplementationType?.GetPrettyName() ?? "a factory or instance registration";

            throw new InvalidOperationException(
                $"Service '{descriptor.ServiceType.GetPrettyName()}' already has the handler '{currentName}'; " +
                $"cannot register '{descriptor.ImplementationType?.GetPrettyName()}'. A request or stream must have exactly one handler."
            );
        }

        // Pipeline behaviors keep the order they were registered in, it is
        // the order they are executed.
        private void RegisterPipelineBehaviors(Type service, IReadOnlyCollection<Type> implementations) {
            self.TryAddEnumerable(CreateDescriptors(service, implementations));
        }
    }

    internal static ServiceDescriptor[] CreateDescriptors(Type service, IReadOnlyCollection<Type> implementations) {
        return [
            .. from implementation in implementations
               from serviceType in GetServiceTypes(implementation, service)
               select ServiceDescriptor.Transient(serviceType, implementation)
        ];
    }

    // Manual registration (MediatorRegistration.With*) already rejects
    // invalid types, so an invalid type reaching this point was found by the
    // assembly scan: point the user to the way of excluding it.
    private static Type[] GetServiceTypes(Type implementation, Type service) {
        try { return MediatorTypeInspector.GetServiceTypes(implementation, service); }
        catch (InvalidOperationException ex) {
            throw new InvalidOperationException(
                $"{ex.Message} If '{implementation.GetPrettyName()}' was found by the assembly scan and must not be registered, " +
                "mark it with [IgnoreAssemblyScan] or make it abstract.",
                ex
            );
        }
    }
}
