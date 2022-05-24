namespace Nameless.EventSourcing.Domains {

    public sealed class AggregateFactory : IAggregateFactory {

		#region IAggregateFactory Members

		/// <inheritdoc />
		public TAggregate? Create<TAggregate>(params object[] args) where TAggregate : AggregateRoot {
			return Activator.CreateInstance(typeof(TAggregate), args) as TAggregate;
		}

		#endregion
	}
}
