using System.Collections;

namespace Nameless.Collections.Generic {

    /// <summary>
    /// Represents a page of enumerable items.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Page<T> : IPage<T> {

        #region Public Static Read-Only Fields

        /// <summary>
        /// Gets a empty page of the defined type <see cref="T"/>.
        /// </summary>
        public static readonly Page<T> Empty = new(Array.Empty<T>());

        #endregion

        #region Private Properties

        private T[] Items { get; }

        #endregion

        #region Public Constructors

        public Page(T[] items, int index = 0, int size = 10, int total = 0) {
            Prevent.Null(items, nameof(items));

            Items = items;
            Index = index >= 0 ? index : 0;
            Size = size > 0 ? size : 10;
            Total = total > 0
                ? (total >= items.Length)
                    ? total
                    : items.Length
                : 0;
        }

        #endregion

        #region IPage<T> Members

        public int Index { get; }

        public int Number => Index + 1;

        public int Size { get; }

        public int Count => Items.Length;

        public int Pages => (int)Math.Ceiling(Total / (decimal)Size);

        public int Total { get; }

        public bool HasNext => Number < Count;

        public bool HasPrevious => Number > 1;

        #endregion

        #region IEnumerable<T> Members

        /// <inheritdocs />
        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerator<T>)Items.GetEnumerator());
        }

        /// <inheritdocs />
        IEnumerator IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    /// <see cref="Page{T}"/> extension methods.
    /// </summary>
    public static class PageExtension {

        #region Public Static Methods

        /// <summary>
        /// Converts the <see cref="IQueryable{T}"/> source into a page.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="self">The source.</param>
        /// <param name="index">The page index. Default is 0 (zero).</param>
        /// <param name="size">The page size. Default is 10.</param>
        /// <returns>An instance of <see cref="Page{T}"/>.</returns>
        public static Page<T> AsPage<T>(this IQueryable<T> self, int index = 0, int size = 10) {
            if (self == null) { return Page<T>.Empty; }

            index = index >= 0 ? index : 0;
            size = size > 0 ? size : 10;

            var total = self.Count();
            var items = self.Skip(index * size).Take(size).ToArray();

            return new Page<T>(items, index, size, total);
        }

        #endregion
    }
}
