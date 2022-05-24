namespace Nameless.EventSourcing.Domains {

    public interface IAggregateFactory {

        #region Methods

        /// <summary>
        /// Creates an aggregate.
        /// </summary>
        /// <param name="args">Arguments passed to the object creation cycle.</param>
        /// <returns>An instance of the aggregate.</returns>
        TAggregate? Create<TAggregate>(params object[] args) where TAggregate : AggregateRoot;

        #endregion
    }
}
