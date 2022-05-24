namespace Nameless.Logging {

    [NullObject]
	public sealed class NullLogger : ILogger {

		#region Public Static Properties

		/// <summary>
		/// Gets the unique instance of NullLogger.
		/// </summary>
		public static ILogger Instance { get; } = new NullLogger();

		#endregion

		#region Static Constructors

		// Explicit static constructor to tell the C# compiler
		// not to mark type as beforefieldinit
		static NullLogger() { }

		#endregion

		#region Private Constructors

		// Prevents the class from being constructed.
		private NullLogger() { }

		#endregion Private Constructors

		#region ILogger Members

		/// <inheritdoc />
		public bool IsEnabled(LogLevel level) => false;

		/// <inheritdoc />
		public void Log(LogLevel level, Exception? exception, string format, params object[] args) { }

		#endregion
	}
}