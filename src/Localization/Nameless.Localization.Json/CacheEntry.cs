using Nameless.Localization.Json.Schema;

namespace Nameless.Localization.Json {

    internal class CacheEntry : IDisposable {

        #region Private Fields

        private TranslationGroup? _group;
        private IDisposable? _changeMonitor;
        private bool _disposed;

        #endregion

        #region Internal Properties

        internal TranslationGroup Group => _group!;

        #endregion

        #region Internal Constructors

        internal CacheEntry(TranslationGroup? group, IDisposable? changeMonitor = null) {
            _group = group ?? throw new ArgumentNullException(nameof(group));
            _changeMonitor = changeMonitor;
        }

        #endregion

        #region Destructor

        ~CacheEntry() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _changeMonitor?.Dispose();
            }

            _group = null;
            _changeMonitor = null;
            _disposed = true;
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
