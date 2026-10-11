using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Nameless.Helpers;
using Nameless.ProducerConsumer.RabbitMQ.Infrastructure;
using Nameless.ProducerConsumer.RabbitMQ.Options;

namespace Nameless.ProducerConsumer.RabbitMQ;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class RabbitMQServiceCollectionExtensions {
    /// <param name="self">
    ///     The current <see cref="IServiceCollection"/>.
    /// </param>
    extension(IServiceCollection self) {
        /// <summary>
        ///     Register the Producer/Consumer services for RabbitMQ.
        /// </summary>
        /// <param name="configure">
        ///     The registration delegate.
        /// </param>
        /// <param name="configuration">
        ///     The configuration.
        /// </param>
        /// <returns>
        ///     The current <see cref="IServiceCollection"/> so other actions
        ///     ca be chained.
        /// </returns>
        public IServiceCollection RegisterProducerConsumer(Action<ProducerConsumerRegistration>? configure = null, IConfiguration? configuration = null) {
            var registration = ActionHelper.FromDelegate(configure);
            self.ConfigureOptions<RabbitMQOptions>(configuration);

            self.TryAddSingleton<IConnectionManager, ConnectionManager>();
            self.TryAddSingleton<IChannelFactory, ChannelFactory>();
            self.TryAddSingleton<IProducer, Producer>();
            self.TryAddSingleton<IMessageSerializer, JsonMessageSerializer>();

            self.TryAddEnumerable(
                registration.Consumers.Select(
                    implementation => ServiceDescriptor.Singleton(
                        typeof(IHostedService),
                        implementation
                    )
                )
            );

            return self;
        }
    }
}