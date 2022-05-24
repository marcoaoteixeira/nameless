namespace Nameless.Environment {

    /// <summary>
    /// Defines methods to expose the application hosting environment.
    /// </summary>
    public interface IHostEnvironment {

        #region Properties

        /// <summary>
        /// Gets the environment name.
        /// </summary>
        string EnvironmentName { get; }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the application base path.
        /// </summary>
        string ApplicationBasePath { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the application shared data.
        /// </summary>
        /// <param name="key">The data key.</param>
        /// <returns>The data.</returns>
        object? GetData(string key);

        /// <summary>
        /// Sets a value to the application shared data storage.
        /// </summary>
        /// <param name="key">The data key.</param>
        /// <param name="data">The data.</param>
        void SetData(string key, object data);

        /// <summary>
        /// Gets the environment variable
        /// </summary>
        /// <param name="key">The variable key.</param>
        /// <param name="target">The variable target (default <see cref="VariableTarget.User" />)</param>
        /// <returns>The variable.</returns>
        string? GetVariable(string key, VariableTarget target = VariableTarget.User);

        /// <summary>
        /// Gets the environment variables
        /// </summary>
        /// <param name="target">The variable target</param>
        /// <returns>The variables.</returns>
        IDictionary<string, string?> GetVariables(VariableTarget target = VariableTarget.User);

        /// <summary>
        /// Sets a variable to the environment.
        /// </summary>
        /// <param name="key">The variable key.</param>
        /// <param name="variable">The variable.</param>
        /// <param name="target">The variable target (default <see cref="VariableTarget.User" />)</param>
        void SetVariable(string key, string variable, VariableTarget target = VariableTarget.User);

        #endregion
    }
}