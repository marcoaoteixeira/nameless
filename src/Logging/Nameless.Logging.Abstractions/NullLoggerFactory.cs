namespace Nameless.Logging {

    [NullObject]
	public sealed class NullLoggerFactory : ILoggerFactory {

		#region Public Static Properties

		/// <summary>
		/// Gets the unique instance of NullLoggerFactory.
		/// </summary>
		public static ILoggerFactory Instance { get; } = new NullLoggerFactory();

		#endregion

		#region Static Constructors

		// Explicit static constructor to tell the C# compiler
		// not to mark type as beforefieldinit
		static NullLoggerFactory() { }

		#endregion

		#region Private Constructors

		// Prevents the class from being constructed.
		private NullLoggerFactory() { }

		#endregion

		#region ILoggerFactory Members

		/// <inheritdoc />
		public ILogger CreateLogger(Type type) => NullLogger.Instance;

		#endregion
	}
}