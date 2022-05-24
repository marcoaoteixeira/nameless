namespace Nameless.Messenger.Email {

    /// <summary>
    /// The configuration for mailing client.
    /// </summary>
    public sealed class MessengerOptions {

		#region Public Enumerators

		/// <summary>
		/// Enumerates the delivery methods.
		/// </summary>
		public enum DeliveryMethods {
			/// <summary>
			/// Network
			/// </summary>
			Network = 0,

			/// <summary>
			/// Pickup Directory
			/// </summary>
			PickupDirectory = 1
		}

		#endregion

		#region Public Static Properties

		public static MessengerOptions Default => new();

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the SMTP server address. Default value is "localhost".
		/// </summary>
		public string Host { get; set; } = "localhost";

		/// <summary>
		/// Whether if will use port, or not use, to connecto to the SMTP
		/// service. Default value is <c>true</c>.
		/// </summary>
		public bool UsePort { get; set; } = true;

		/// <summary>
		/// Gets or sets the SMTP server port. Default value is 25.
		/// </summary>
		public int Port { get; set; } = 25;

		/// <summary>
		/// Gets or sets whether should use credentials. Default value is
		/// <c>false</c>.  
		/// </summary>
		public bool UseCredentials { get; set; }

		/// <summary>
		/// Gets or sets the user name credential.
		/// </summary>
		public string? UserName { get; set; }

		/// <summary>
		/// Gets or sets the password credential.
		/// </summary>
		public string? Password { get; set; }

		/// <summary>
		/// Gets or sets whether should enable SSL. Default value is
		/// <c>false</c>.
		/// </summary>
		public bool EnableSsl { get; set; }

		/// <summary>
		/// Gets or sets the delivery method. Default value is
		/// <see cref="DeliveryMethods.PickupDirectory" />.
		/// </summary>
		public DeliveryMethods DeliveryMethod { get; set; } = DeliveryMethods.PickupDirectory;

		/// <summary>
		/// Gets or sets the pickup directory path, relative to the
		/// application file storage. Default value is
		/// "Mailing/PickupDirectory".
		/// </summary>
		public string PickupDirectoryFolder { get; set; } = Path.Combine("Messenger_Email", "PickupDirectory");

		#endregion
	}
}
