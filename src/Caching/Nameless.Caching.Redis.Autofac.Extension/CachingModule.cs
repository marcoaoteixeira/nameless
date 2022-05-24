using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;
using StackExchange.Redis;

namespace Nameless.Caching.Redis {

    public sealed class CachingModule : ModuleBase {

		#region Private Constants

		private const string CONNECTION_MULTIPLEXER_KEY = "a2ac7b95-4c59-4fab-9c38-79ffe37aeeef";

		#endregion

		#region Protected Override Methods

		protected override void Load(ContainerBuilder builder) {
			builder
				.Register(RegisterConnectionMultiplexer)
				.Named<IConnectionMultiplexer>(CONNECTION_MULTIPLEXER_KEY)
				.SetLifetimeScope(LifetimeScopeType.Singleton);

			builder
				.Register<ICache, RedisCache>(
					lifetimeScope: LifetimeScopeType.Singleton,
					parameters: new[] { ResolvedParameter.ForNamed<IConnectionMultiplexer>(CONNECTION_MULTIPLEXER_KEY) }
				);

			base.Load(builder);
		}

		#endregion

		#region Private Static Methods

		private static IConnectionMultiplexer RegisterConnectionMultiplexer(IComponentContext ctx) {
			var opts = ctx.ResolveOptional<CacheOptions>() ?? new();
			return ConnectionMultiplexer.Connect(string.Join(",", opts.Servers));
		}

		#endregion
	}
}
