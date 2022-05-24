using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Nameless {

    public static class Ensure {

        #region Public Static Methods

        /// <summary>
        /// Makes sure that the <paramref name="value"/> is not <c>null</c>.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <c>null</c>.
        /// </exception>
        [DebuggerStepThrough]
        public static void NotNull([NotNull] object? value, string? paramName) {
            if (value == null) {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Makes sure that the <paramref name="value"/> is not <c>null</c>,
        /// empty or white spaces.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is empty or white spaces.
        /// </exception>
        [DebuggerStepThrough]
        public static void NotNullEmptyOrWhiteSpace([NotNull] string? value, string? paramName) {
            NotNull(value, paramName);

            if (value.Trim().Length == 0) {
                throw new ArgumentException("Value cannot be empty or white spaces.", paramName);
            }
        }

        /// <summary>
        /// Makes sure that the <paramref name="value"/> is not
        /// <c>null</c> or empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> is empty.
        /// </exception>
		[DebuggerStepThrough]
        public static void NotNullOrEmpty([NotNull] IEnumerable? value, string? paramName) {
            NotNull(value, paramName);

            // Costs O(1)
            if (value is ICollection collection && collection.Count == 0) {
                throw new ArgumentException("Value cannot be empty.", paramName);
            }

            // Costs O(N)
            var enumerator = value.GetEnumerator();
            var canMoveNext = enumerator.MoveNext();
            if (enumerator is IDisposable disposable) {
                disposable.Dispose();
            }
            if (!canMoveNext) {
                throw new ArgumentException("Value cannot be empty.", paramName);
            }
        }

        /// <summary>
		/// Makes sure that the <paramref name="specificType"/> is
        /// assignable from the <paramref name="baseType"/>.
		/// </summary>
		/// <param name="baseType">The base type.</param>
		/// <param name="specificType">The specific type.</param>
		/// <exception cref="ArgumentNullException">
        /// if <paramref name="baseType"/> is <c>null</c>.
        /// </exception>
		/// <exception cref="ArgumentNullException">
        /// if <paramref name="specificType"/> is <c>null</c>.
        /// </exception>
		/// <exception cref="ArgumentException">
		/// if <paramref name="baseType"/> is not assignable
        /// from <paramref name="specificType"/>.
		/// </exception>
		[DebuggerStepThrough]
        public static void TypeAssignableFrom(Type baseType, Type specificType) {
            NotNull(baseType, nameof(baseType));
            NotNull(specificType, nameof(specificType));

            if (!baseType.IsAssignableFrom(specificType)) {
                throw new ArgumentException($"The specified type ({specificType}) must be assignable to {baseType}");
            }
        }

        #endregion
    }
}
