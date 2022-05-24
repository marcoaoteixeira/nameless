namespace Nameless {

    /// <summary>
    /// <see cref="Task"/> extension methods.
    /// </summary>
    public static class TaskExtension {

        #region Public Static Methods

        /// <summary>
        /// Checks if the <see cref="Task"/> didn't thrown an exception, was canceled, was not faulted and is completed.
        /// </summary>
        /// <param name="self">The <see cref="Task"/> source</param>
        /// <returns><c>true</c> if can continue; otherwise <c>false</c>.</returns>
        public static bool CanContinue(this Task self) {
            return self.Exception == null && !self.IsCanceled && !self.IsFaulted && self.IsCompleted;
        }

        #endregion
    }
}
