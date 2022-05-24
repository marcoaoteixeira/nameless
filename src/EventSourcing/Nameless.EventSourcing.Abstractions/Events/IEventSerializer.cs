namespace Nameless.EventSourcing.Events {

    public interface IEventSerializer {

        #region Methods

        /// <summary>
        /// Serializes an event.
        /// </summary>
        /// <param name="evt">The event.</param>
        /// <returns>A <see cref="byte[]"/> representing serialized event.</returns>
        byte[]? Serialize(IEvent evt);

        /// <summary>
        /// Deserializes an event.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="payload">The payload.</param>
        /// <returns>A <see cref="IEvent"/> implemented instance representing the deserialized <see cref="byte[]"/>.</returns>
        IEvent? Deserialize(Type eventType, byte[] payload);

        #endregion
    }
}
