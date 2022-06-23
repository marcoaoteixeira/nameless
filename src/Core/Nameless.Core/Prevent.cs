using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Nameless {

    public static class Prevent {

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
        public static void Null([NotNull] object? value, string? paramName) {
            if (value == null) {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Makes sure that the <paramref name="value"/> is not <c>default</c>.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="paramName">The parameter name.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="value"/> is <c>null</c>.
        /// </exception>
        [DebuggerStepThrough]
        public static void Default<T>([NotNull] T value, string? paramName) where T : struct {
            if (default(T).Equals(value)) {
                throw new ArgumentException("Parameter cannot be default value.", paramName);
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
        public static void NullEmptyOrWhiteSpace([NotNull] string? value, string? paramName) {
            Null(value, paramName);

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
        public static void NullOrEmpty([NotNull] IEnumerable? value, string? paramName) {
            Null(value, paramName);

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
        public static void TypeNotAssignableFrom(Type baseType, Type specificType) {
            Null(baseType, nameof(baseType));
            Null(specificType, nameof(specificType));

            if (!baseType.IsAssignableFrom(specificType)) {
                throw new ArgumentException($"The specified type ({specificType}) must be assignable to {baseType}");
            }
        }

        #endregion
    }
}
