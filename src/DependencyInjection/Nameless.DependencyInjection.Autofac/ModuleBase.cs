using System.Reflection;
using Autofac;

namespace Nameless.DependencyInjection.Autofac {

    /// <summary>
    /// Extended abstract class <see cref="global::Autofac.Module" />.
    /// </summary>
    public abstract class ModuleBase : global::Autofac.Module, IModule {

        #region Protected Properties

        /// <summary>
        /// Gets the support assemblies.
        /// </summary>
        protected IEnumerable<Assembly> SupportAssemblies { get; }

        #endregion

        #region Protected Constructors

        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="supportAssemblies">The support assemblies.</param>
        protected ModuleBase(params Assembly[] supportAssemblies) {
            SupportAssemblies = supportAssemblies ?? Array.Empty<Assembly>();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Searches for an implementation for a given service type.
        /// </summary>
        /// <typeparam name="TType">The service type.</typeparam>
        /// <returns>The service type implementation</returns>
        protected Type? SearchForImplementation<TType>() {
            return SearchForImplementation(typeof(TType));
        }

        /// <summary>
        /// Searches for implementations for a given service type.
        /// </summary>
        /// <typeparam name="TType">The service type.</typeparam>
        /// <returns>An array of types</returns>
        protected Type[] SearchForImplementations<TType>() {
            return SearchForImplementations(typeof(TType));
        }

        /// <summary>
        /// Searches for an implementation for a given service type.
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <returns>The service type implementation</returns>
        protected Type? SearchForImplementation(Type serviceType) {
            return SearchForImplementations(serviceType).SingleOrDefault();
        }

        /// <summary>
        /// Searches for implementations for a given service type.
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <returns>An array of types</returns>
        protected Type[] SearchForImplementations(Type serviceType) {
            Ensure.NotNull(serviceType, nameof(serviceType));

            if (!SupportAssemblies.Any()) { return Enumerable.Empty<Type>().ToArray(); }

            var result = SupportAssemblies
                .SelectMany(assembly => assembly.GetExportedTypes())
                .Where(type => !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface)
                .Where(type => serviceType.IsAssignableFrom(type) || type.IsAssignableToGenericType(serviceType))
                .Where(type => type.GetCustomAttribute<NullObjectAttribute>(inherit: false) == null)
                .ToArray();

            return result;
        }

        #endregion

        #region IModule Members

        void IModule.Configure(IServiceComposer composer) {
            throw new NotImplementedException();
        }

        #endregion
    }
}