namespace Nameless.Localization.Json.Schema {

    public sealed class TranslationCollection {

        #region Private Read-Only Fields

        private readonly Dictionary<string, Translation> _dictionary;

        #endregion

        #region Public Properties

        public string Key { get; }
        public Translation[] Values => _dictionary.Values.ToArray();

        #endregion

        #region Public Constructors

        public TranslationCollection(string key, IEnumerable<Translation>? values = null) {
            Ensure.NotNullEmptyOrWhiteSpace(key, nameof(key));

            Key = key;
            _dictionary = (values ?? Enumerable.Empty<Translation>()).ToDictionary(_ => _.Key, _ => _);
        }

        #endregion

        #region Public Methods

        public bool TryGetValue(string key, out Translation? output) => _dictionary.TryGetValue(key, out output);

        public bool Equals(TranslationCollection? other) => other != null && other.Key == Key;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as TranslationCollection);

        public override int GetHashCode() => (Key ?? string.Empty).GetHashCode();

        #endregion
    }
}
