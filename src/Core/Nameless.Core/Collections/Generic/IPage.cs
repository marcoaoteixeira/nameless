namespace Nameless.Collections.Generic {

    /// <summary>
    /// Defines the contract to a page.
    /// </summary>
    /// <typeparam name="T">Type of the page.</typeparam>
    public interface IPage<T> : IEnumerable<T> {

        #region Properties

        /// <summary>
        /// Gets the index of the page.
        /// </summary>
        int Index { get; }
        /// <summary>
        /// Gets the number of the page.
        /// </summary>
        int Number { get; }
        /// <summary>
        /// Gets the page size, how many items it can display.
        /// </summary>
        int Size { get; }
        /// <summary>
        /// Gets the total of items in this page.
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Gets the total of pages that the collection of items can yeild.
        /// </summary>
        int Pages { get; }
        /// <summary>
        /// Gets the total of items in the collection that gives origin to the page.
        /// </summary>
        int Total { get; }
        /// <summary>
        /// Whether if there is a next page.
        /// </summary>
        bool HasNext { get; }
        /// <summary>
        /// Whether if there is a previous page.
        /// </summary>
        bool HasPrevious { get; }

        #endregion
    }
}
