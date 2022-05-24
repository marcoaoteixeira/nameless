using System.Reflection;
using Autofac;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.CommandQuery {

    public sealed class CommandQueryModule : ModuleBase {

        #region Public Properties

        /// <summary>
        /// Gets or sets the <see cref="ICommandHandler{TCommand}"/> implementations.
        /// </summary>
        public Type[] CommandHandlerImplementations { get; set; } = Array.Empty<Type>();

        /// <summary>
        /// Gets or sets the <see cref="IQueryHandler{TQuery, TResult}"/> implementations.
        /// </summary>
        public Type[] QueryHandlerImplementations { get; set; } = Array.Empty<Type>();

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CQRSModule"/>.
        /// </summary>
        /// <param name="supportAssemblies">The support assemblies.</param>
        public CommandQueryModule(params Assembly[] supportAssemblies) : base(supportAssemblies) { }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder.Register<IDispatcher, Dispatcher>(lifetimeScope: LifetimeScopeType.Singleton);

            builder
                .RegisterTypes(CommandHandlerImplementations ?? SearchForImplementations(typeof(ICommandHandler<>)))
                .AsClosedTypesOf(typeof(ICommandHandler<>))
                .SetLifetimeScope(LifetimeScopeType.Transient);

            var queryHandlerImplementations = QueryHandlerImplementations ?? SearchForImplementations(typeof(IQueryHandler<,>));
            var queryImplementations = queryHandlerImplementations
                .Select(ProjectQuery)
                .Where(_ => _ != null)
                .ToArray();
            builder
                .RegisterTypes(queryImplementations!)
                .AsClosedTypesOf(typeof(IQuery<>))
                .SetLifetimeScope(LifetimeScopeType.Transient);

            builder
                .RegisterTypes(queryHandlerImplementations)
                .AsClosedTypesOf(typeof(IQueryHandler<,>))
                .SetLifetimeScope(LifetimeScopeType.Transient);

            base.Load(builder);
        }

        #endregion

        #region Private Static Methods

        private static Type? ProjectQuery(Type type) {
            var @interface = type
                .GetInterfaces()
                .FirstOrDefault(_ => _.IsAssignableToGenericType(typeof(IQueryHandler<,>)));
            if (@interface == null) { return default; }
            var arg = @interface
                .GetGenericArguments()
                .FirstOrDefault(_ => _.IsAssignableToGenericType(typeof(IQuery<>)));
            return arg;
        }

        #endregion
    }
}
