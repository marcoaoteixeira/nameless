using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Nameless.DependencyInjection.Autofac;

namespace Nameless.Logging.Log4net {
	/// <summary>
	/// The logging service registration.
	/// </summary>
	public sealed class LoggingModule : ModuleBase {

		#region Protected Override Methods

		/// <inheritdoc/>
		protected override void Load(ContainerBuilder builder) {
			builder.Register<ILoggerFactory, LoggerFactory>(lifetimeScope: LifetimeScopeType.Singleton);
		}

		/// <inheritdoc/>
		protected override void AttachToComponentRegistration(IComponentRegistryBuilder componentRegistry, IComponentRegistration registration) {
			registration.PipelineBuilding += (sender, pipeline) => {
				pipeline.Use(new FactoryResolveMiddleware(
					injectType: typeof(ILogger),
					factory: (member, ctx) => {
						return member.DeclaringType != null
							? ctx.Resolve<ILoggerFactory>().CreateLogger(member.DeclaringType)
							: NullLogger.Instance;
					}
				));
			};

			base.AttachToComponentRegistration(componentRegistry, registration);
		}

		#endregion
	}
}
