namespace Nameless.Logging {

	/// <summary>
	/// Extension methods for <see cref="LogLevel"/>
	/// </summary>
	public static class LogLevelExtension {

		#region Public Static Methods

		/// <summary>
		/// Checks if the specified flag is setted on the current flag.
		/// </summary>
		/// <param name="self">The source (<see cref="LogLevel"/>).</param>
		/// <param name="flags">The flag (<see cref="LogLevel"/>).</param>
		/// <returns><c>true</c> if is setted, otherwise, <c>false</c>.</returns>
		public static bool IsSet(this LogLevel self, LogLevel flags) {
			return (self & flags) == flags;
		}

		#endregion
	}
}