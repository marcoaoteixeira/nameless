using System;

namespace Nameless.EventSourcing.Events {

    /// <summary>
    /// Represents an event.
    /// </summary>
    public interface IEvent {

        #region Public Properties

        /// <summary>
        /// Gets or sets the event ID.
        /// </summary>
        Guid EventID { get; set; }

        /// <summary>
        /// Gets or sets the aggregate ID.
        /// </summary>
        Guid AggregateID { get; set; }

        /// <summary>
        /// Gets or sets the event version.
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// Gets or sets the event time stamp.
        /// </summary>
        DateTimeOffset TimeStamp { get; set; }

        #endregion
    }
}
