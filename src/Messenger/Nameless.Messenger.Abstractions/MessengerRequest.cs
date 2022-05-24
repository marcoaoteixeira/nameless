using System.Text;

namespace Nameless.Messenger {

    /// <summary>
    /// The request.
    /// </summary>
    public sealed class MessengerRequest {

		#region Public Properties

		/// <summary>
		/// Gets or sets the message subject.
		/// </summary>
		public string? Subject { get; set; }
		/// <summary>
		/// Gets or sets the message content.
		/// </summary>
		public string? Message { get; set; }
		/// <summary>
		/// Gets or sets the message language.
		/// </summary>
		public string? Language { get; set; }
		/// <summary>
		/// Gets or sets the message encoding.
		/// </summary>
		public Encoding Encoding { get; set; } = Encoding.UTF8;
		/// <summary>
		/// Gets or sets an array of address from the
		/// person (or people) who sends the message
		/// </summary>
		public string[]? From { get; set; }
		/// <summary>
		/// Gets or sets an array of address to the
		/// person (or people) who receives the message
		/// </summary>
		public string[]? To { get; set; }
		/// <summary>
		/// A dictionary of properties that can be used
		/// by the messenger.
		/// </summary>
		public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();
		/// <summary>
		/// Gets or sets the message priority.
		/// </summary>
		public MessagePriority Priority { get; set; }

        #endregion
    }
}
