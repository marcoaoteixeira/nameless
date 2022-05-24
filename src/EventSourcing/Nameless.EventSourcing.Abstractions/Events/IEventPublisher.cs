using System.Threading;
using System.Threading.Tasks;

namespace Nameless.EventSourcing.Events {

    /// <summary>
    /// Event publisher.
    /// </summary>
    public interface IEventPublisher {

        #region Methods

        /// <summary>
        /// Publishes an event.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task" /> representing the method execution.</returns>
        Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default) where TEvent : IEvent;

        #endregion Methods
    }
}
