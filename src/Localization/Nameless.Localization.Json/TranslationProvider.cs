using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using Nameless.FileStorage;
using Nameless.Localization.Json.Schema;
using Newtonsoft.Json;

namespace Nameless.Localization.Json {

    public sealed class TranslationProvider : ITranslationProvider, IDisposable {

        #region Private Read-Only Fields

        private readonly IFileStorage _fileStorage;
        private readonly LocalizationOptions _options;

        #endregion

        #region Private Fields

        private ConcurrentDictionary<string, CacheEntry>? _cache = new();
        private bool _disposed;

        #endregion

        #region Public Constructors

        public TranslationProvider(IFileStorage fileStorage, LocalizationOptions? options = null) {
            Ensure.NotNull(fileStorage, nameof(fileStorage));

            _fileStorage = fileStorage;
            _options = options ?? new();
        }

        #endregion

        #region Destructor

        ~TranslationProvider() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void ChangeMonitorCallback(ChangeEventArgs args) {
            var culture = Path.GetFileNameWithoutExtension(args.OriginalPath)!.ToLower(); // normalize

            if (!string.IsNullOrWhiteSpace(culture)) {
                if (_cache!.TryRemove(culture, out CacheEntry? entry)) {
                    entry.Dispose();
                }
            }
        }

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                lock (_cache!) {
                    foreach (var entry in _cache.Values) {
                        entry.Dispose();
                    }
                }
            }

            _cache!.Clear();
            _cache = null;
            _disposed = true;
        }

        #endregion

        #region ITranslationStorage Members

        public async Task<TranslationGroup?> GetAsync(CultureInfo? culture, CancellationToken cancellationToken = default) {
            BlockAccessAfterDispose();

            if (culture == null) { throw new ArgumentNullException(nameof(culture)); }

            // Retrieves the associated file
            var filePath = Path.Combine(_options.ResourceFolderPath, $"{culture.Name}.json");
            var file = await _fileStorage.GetFileAsync(filePath);

            cancellationToken.ThrowIfCancellationRequested();

            // If file not exists, return.
            if (file == null || !file.Exists) { return new TranslationGroup(culture); }

            var entry = _cache!.GetOrAdd(culture.Name, key => {
                var json = file.GetText(Encoding.UTF8); /* always read files as UTF-8 */
                var translationCollections = JsonConvert.DeserializeObject<TranslationCollection[]>(json!, new TranslationCollectionJsonConverter());
                var translationGroup = new TranslationGroup(culture, translationCollections);

                // Keep an eye in the file changed event, if needed.
                var changeMonitor = _options.ReloadOnChange ? file.Watch(ChangeMonitorCallback) : null;

                return new CacheEntry(translationGroup, changeMonitor);
            });

            return entry.Group;
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
