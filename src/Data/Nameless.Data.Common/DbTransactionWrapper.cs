using System.Data;

namespace Nameless.Data {

    public sealed class DbTransactionWrapper : IDbTransaction {

        #region Private Fields

        private IDbTransaction? _inner;
        private bool _disposed;

        #endregion

        #region Public Events

        public event EventHandler<EventArgs>? Committed;
        public event EventHandler<EventArgs>? Rolledback;
        public event EventHandler<EventArgs>? Disposed;

        #endregion

        #region Public Constructors

        public DbTransactionWrapper(IDbTransaction transaction) {
            Ensure.NotNull(transaction, nameof(transaction));

            _inner = transaction;
        }

        #endregion

        #region Destructor

        ~DbTransactionWrapper() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                if (_inner != null) {
                    _inner.Dispose();
                }
            }

            _inner = null;
            _disposed = true;

            OnDispose();
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void OnCommit() {
            Committed?.Invoke(this, EventArgs.Empty);
        }

        private void OnRollback() {
            Rolledback?.Invoke(this, EventArgs.Empty);
        }

        private void OnDispose() {
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region IDbTransaction Members

        public IDbConnection? Connection => _inner?.Connection;
        public IsolationLevel IsolationLevel => _inner != null ? _inner.IsolationLevel : IsolationLevel.Unspecified;

        public void Commit() {
            BlockAccessAfterDispose();

            _inner!.Commit();

            OnCommit();
        }

        public void Rollback() {
            BlockAccessAfterDispose();

            _inner!.Rollback();

            OnRollback();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}