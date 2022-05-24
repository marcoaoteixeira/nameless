using Autofac;
using Autofac_Parameter = Autofac.Core.Parameter;

namespace Nameless.DependencyInjection.Autofac {

    public sealed class ServiceComposer : IServiceComposer {

        #region Public Properties

        public ContainerBuilder Builder { get; }

        #endregion

        #region Public Constructors

        public ServiceComposer(ContainerBuilder? builder = null) {
            Builder = builder ?? new ContainerBuilder();
        }

        #endregion

        #region Private Methods

        private IServiceComposer AddService(Type serviceType, Type implementationType, LifetimeScopeType scopeType, IEnumerable<Parameter> parameters) {
            var registration = Builder
                .RegisterType(implementationType)
                .As(serviceType);

            foreach (var parameter in parameters) {
                Autofac_Parameter? af_parameter = null;

                if (parameter.Type != null) {
                    af_parameter = new TypedParameter(parameter.Type, parameter.Value);
                }

                if (!string.IsNullOrWhiteSpace(parameter.Name) && parameter.Value != null) {
                    af_parameter = new NamedParameter(parameter.Name, parameter.Value);
                }

                if (af_parameter == null) { continue; }

                registration.WithParameter(af_parameter);
            }

            registration.SetLifetimeScope(scopeType);

            return this;
        }

        #endregion

        #region IServiceComposer Members

        public IServiceComposer AddPerScope(Type serviceType, Type implementationType, params Parameter[] parameters) {
            return AddService(serviceType, implementationType, LifetimeScopeType.PerScope, parameters);
        }

        public IServiceComposer AddSingleton(Type serviceType, Type implementationType, params Parameter[] parameters) {
            return AddService(serviceType, implementationType, LifetimeScopeType.Singleton, parameters);
        }

        public IServiceComposer AddTransient(Type serviceType, Type implementationType, params Parameter[] parameters) {
            return AddService(serviceType, implementationType, LifetimeScopeType.Transient, parameters);
        }

        public IServiceComposer AddModule(Type moduleType) {
            if (moduleType == null) { throw new ArgumentNullException(nameof(moduleType)); }

            if (Activator.CreateInstance(moduleType) is not ModuleBase module) {
                throw new InvalidOperationException("Could not load module.");
            }

            Builder.RegisterModule(module);

            return this;
        }

        public IServiceComposer AddModule(IModule module) {
            Ensure.NotNull(module, nameof(module));

            if (module is not ModuleBase moduleBase) {
                throw new InvalidOperationException($"Parameter {nameof(module)} must implement {typeof(ModuleBase)}");
            }

            Builder.RegisterModule(moduleBase);

            return this;
        }

        #endregion
    }
}