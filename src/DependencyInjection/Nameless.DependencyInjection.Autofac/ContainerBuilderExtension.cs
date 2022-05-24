using Autofac;
using Autofac_Parameter = Autofac.Core.Parameter;

namespace Nameless.DependencyInjection.Autofac {

    public static class ContainerBuilderExtension {

        #region Public Static Methods

        public static ContainerBuilder Register<TService, TImplementation>(this ContainerBuilder self, string? name = null, LifetimeScopeType lifetimeScope = LifetimeScopeType.Transient, params Autofac_Parameter[] parameters)
            where TService : class
            where TImplementation : TService {
            return Register(
                self: self,
                service: typeof(TService),
                implementation: typeof(TImplementation),
                name: name,
                lifetimeScope: lifetimeScope,
                parameters: parameters
            );
        }

        public static ContainerBuilder Register(this ContainerBuilder self, Type service, Type implementation, string? name = null, LifetimeScopeType lifetimeScope = LifetimeScopeType.Transient, params Autofac_Parameter[] parameters) {
            if (self == null) { throw new ArgumentNullException(nameof(self)); }

            Ensure.TypeAssignableFrom(service, implementation);

            if (RegisterAsSingleton(self, service, implementation, name)) { return self; }

            var registration = self.RegisterType(implementation);
            registration = !string.IsNullOrWhiteSpace(name)
                    ? registration.Named(name, service)
                    : registration.As(service);
            if (!parameters.IsNullOrEmpty()) {
                registration.WithParameters(parameters);
            }
            registration.SetLifetimeScope(lifetimeScope);
            return self;
        }

        #endregion

        #region Private Static Methods

        private static bool RegisterAsSingleton(ContainerBuilder builder, Type service, Type implementation, string? name = null) {
            if (SingletonAttribute.IsSingleton(implementation)) {
                var registration = builder.RegisterInstance(SingletonAttribute.GetInstance(implementation)!);
                registration = !string.IsNullOrWhiteSpace(name)
                    ? registration.Named(name, service)
                    : registration.As(service);
                registration.SingleInstance();
                return true;
            }
            return false;
        }

        #endregion
    }
}
