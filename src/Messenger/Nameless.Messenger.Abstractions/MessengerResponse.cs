namespace Nameless.Messenger {

    public sealed class MessengerResponse {

        #region Public Static Read-Only Fields

        public static readonly MessengerResponse Successful = new();

        #endregion

        #region Public Properties

        public Exception? Error { get; set; }
        public bool Success => Error == null;

        #endregion
    }
}
