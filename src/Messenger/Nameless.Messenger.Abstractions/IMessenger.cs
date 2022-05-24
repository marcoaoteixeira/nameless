namespace Nameless.Messenger {

    public interface IMessenger {

		#region Methods

		Task<MessengerResponse> DispatchAsync(MessengerRequest request, CancellationToken cancellationToken = default);

		#endregion
	}
}
