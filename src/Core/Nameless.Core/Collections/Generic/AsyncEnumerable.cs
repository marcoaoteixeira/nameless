namespace Nameless.Collections.Generic {

    public sealed class AsyncEnumerable<T> : IAsyncEnumerable<T> {

        #region Private Read-Only Fields

        private readonly IEnumerable<T> _enumerable;

        #endregion

        #region Public Constructors

        public AsyncEnumerable(IEnumerable<T> enumerable) {
            Ensure.NotNull(enumerable, nameof(enumerable));

            _enumerable = enumerable;
        }

        #endregion

        #region IAsyncEnumerable<T> Members

        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken) {
            return new AsyncEnumerator<T>(_enumerable.GetEnumerator(), cancellationToken);
        }

        #endregion
    }

    public sealed class AsyncEnumerator<T> : IAsyncEnumerator<T> {

        #region Private Read-Only Fields

        private readonly CancellationToken _cancellationToken;

        #endregion

        #region Private Fields

        private IEnumerator<T>? _enumerator;
        private bool _disposed;

        #endregion

        #region Public Constructors

        public AsyncEnumerator(IEnumerator<T> enumerator, CancellationToken cancellationToken = default) {
            Ensure.NotNull(enumerator, nameof(enumerator));

            _enumerator = enumerator;
            _cancellationToken = cancellationToken;
        }

        #endregion

        #region Destructor

        ~AsyncEnumerator() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                if (_enumerator != null) {
                    _enumerator.Dispose();
                }
            }

            _enumerator = null;
            _disposed = true;
        }

        #endregion

        #region IAsyncEnumerator<T> Members

        T IAsyncEnumerator<T>.Current {
            get {
                BlockAccessAfterDispose();

                _cancellationToken.ThrowIfCancellationRequested();
                return _enumerator!.Current;
            }
        }

        ValueTask IAsyncDisposable.DisposeAsync() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);

            return ValueTask.CompletedTask;
        }

        ValueTask<bool> IAsyncEnumerator<T>.MoveNextAsync() {
            BlockAccessAfterDispose();

            _cancellationToken.ThrowIfCancellationRequested();

            return ValueTask.FromResult(_enumerator!.MoveNext());
        }

        #endregion
    }
}
