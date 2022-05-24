using System.Globalization;

namespace Nameless.Helpers {

    /// <summary>
    /// Asynchronous helper.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/aspnet/AspNetIdentity/blob/main/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
    /// </remarks>
    public static class AsyncHelper {

        #region Private Static Read-Only Fields

        private static readonly TaskFactory CurrentTaskFactory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Executes an asynchronous method synchronous.
        /// </summary>
        /// <param name="function">The async method.</param>
        public static void RunSync(Func<Task> function) {
            var currentUICulture = CultureInfo.CurrentUICulture;
            var currentCulture = CultureInfo.CurrentCulture;
            CurrentTaskFactory.StartNew(() => {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
                return function();
            })
            .Unwrap()
            .GetAwaiter()
            .GetResult();
        }

        /// <summary>
        /// Executes an asynchronous method synchronous.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="function">The async method.</param>
        public static TResult RunSync<TResult>(Func<Task<TResult>> function) {
            var currentUICulture = CultureInfo.CurrentUICulture;
            var currentCulture = CultureInfo.CurrentCulture;
            return CurrentTaskFactory.StartNew(() => {
                Thread.CurrentThread.CurrentCulture = currentCulture;
                Thread.CurrentThread.CurrentUICulture = currentUICulture;
                return function();
            })
            .Unwrap()
            .GetAwaiter()
            .GetResult();
        }

        #endregion
    }
}