using Autofac;
using Autofac.Core;
using Nameless.DependencyInjection.Autofac;
using RabbitMQ.Client;

namespace Nameless.PubSub.RabbitMQ {

    /// <summary>
    /// The PubSub service registration.
    /// </summary>
    public sealed class PubSubModule : ModuleBase {

        #region Protected Override Methods

        /// <inheritdoc/>
        protected override void Load(ContainerBuilder builder) {
            builder
                .RegisterType<ConnectionFactory>()
                .As<IConnectionFactory>()
                .OnActivated(OnActivatedConnectionFactory)
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            builder
                .Register<IPublisher, Publisher>(lifetimeScope: LifetimeScopeType.Singleton)
                .Register<ISubscriber, Subscriber>(lifetimeScope: LifetimeScopeType.Singleton);

            base.Load(builder);
        }

        #endregion

        #region Private Static Methods

        private static void OnActivatedConnectionFactory(IActivatedEventArgs<ConnectionFactory> args) {
            var opts = args.Context.ResolveOptional<PubSubOptions>() ?? new();
            var factory = args.Instance;

            factory.HostName = opts.Server.Hostname;
            if (opts.Credentials != null) {
                factory.UserName = opts.Credentials.Username;
                factory.Password = opts.Credentials.Password;
            }
        }

        #endregion
    }
}
