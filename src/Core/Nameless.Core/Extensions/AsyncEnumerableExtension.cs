using System.Runtime.CompilerServices;
using Nameless.Collections.Generic;

namespace Nameless {

    /// <summary>
    /// <see cref="IAsyncEnumerable{T}"/> extension methods.
    /// </summary>
    public static class AsyncEnumerableExtension {

        #region Publci Static Methods

        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> self) => new AsyncEnumerable<T>(self);

        /// <summary>
        /// Retrieves the first or default item of the <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="self">The <see cref="IAsyncEnumerable{T}"/> source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The first or default item in the <see cref="IAsyncEnumerable{T}"/></returns>
        public async static Task<T> FirstOrDefault<T>(this IAsyncEnumerable<T> self, CancellationToken cancellationToken = default) {
            if (self == null) { return default!; }

            await using var enumerator = self.GetAsyncEnumerator(cancellationToken);

            return await enumerator.MoveNextAsync() ? enumerator.Current : default!;
        }

        /// <summary>
        /// Projects an <see cref="IAsyncEnumerable{TInput}"/> into a new <see cref="IAsyncEnumerable{TOutput}"/>
        /// </summary>
        /// <typeparam name="TInput">The input type</typeparam>
        /// <typeparam name="TOutput">The output type</typeparam>
        /// <param name="self">The <see cref="IAsyncEnumerable{TInput}"/> source.</param>
        /// <param name="project">The project function.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An instance of <see cref="IAsyncEnumerable{TOutput}"/></returns>
        public async static IAsyncEnumerable<TOutput> Project<TInput, TOutput>(this IAsyncEnumerable<TInput> self, Func<TInput, TOutput> project, [EnumeratorCancellation] CancellationToken cancellationToken = default) {
            Prevent.Null(project, nameof(project));

            if (self == null) { yield break; }

            await using var enumerator = self.GetAsyncEnumerator(cancellationToken);

            while (await enumerator.MoveNextAsync()) {
                cancellationToken.ThrowIfCancellationRequested();
                yield return project(enumerator.Current);
            }
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> into a <see cref="IList{T}"/>
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="self">The <see cref="IAsyncEnumerable{T}"/> source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An instance of <see cref="IList{T}"/></returns>
        public async static Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> self, CancellationToken cancellationToken = default) {
            if (self == null) { return default!; }

            await using var enumerator = self.GetAsyncEnumerator(cancellationToken);

            IList<T> result = new List<T>();
            while (await enumerator.MoveNextAsync()) {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(enumerator.Current);
            }

            return result;
        }

        #endregion
    }
}
