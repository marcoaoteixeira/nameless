using System.Collections;

namespace Nameless {

    /// <summary>
    /// <see cref="IDictionary{TKey, TValue}"/> extension methods.
    /// </summary>
    public static class DictionaryExtension {

        #region Public Static Methods

        /// <summary>
        /// Adds (or changes) a value onto a dictionary without fuss.
        /// </summary>
        /// <param name="self">The source dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key, TValue value) {
            if (self == null) { return; }
            if (!self.ContainsKey(key)) { self.Add(key, value); } else { self[key] = value; }
        }

        /// <summary>
        /// Tries get a typed version of the value.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <param name="self">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="output">The output value; if any.</param>
        /// <returns><c>true</c> if was possible get the value as the output type; otherwise <c>false</c>.</returns>
        public static bool TryGet<TKey, TValue, TResult>(this IDictionary<TKey, TValue> self, TKey key, out TResult? output) {
            output = default;

            if (self == null) { return false; }
            if (!self.ContainsKey(key)) { return false; }
            if (self[key] is TResult result) { output = result; return true; } else { return false; }
        }

        /// <summary>
        /// Tries get a typed version of the value.
        /// </summary>
        /// <typeparam name="TResult">The type of the expected result.</typeparam>
        /// <param name="self">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="output">The output value; if any.</param>
        /// <returns><c>true</c> if was possible get the value as the output type; otherwise <c>false</c>.</returns>
        public static bool TryGet<TResult>(this IDictionary self, object key, out TResult? output) {
            output = default;

            if (self == null) { return false; }
            if (!self.Contains(key)) { return false; }
            if (self[key] is TResult result) { output = result; return true; } else { return false; }
        }

        #endregion
    }
}