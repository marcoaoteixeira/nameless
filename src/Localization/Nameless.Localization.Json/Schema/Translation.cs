namespace Nameless.Localization.Json.Schema {

    public sealed class Translation {

        #region Public Properties

        public string Key { get; }
        public string[] Values { get; }

        #endregion

        #region Public Constructors

        public Translation(string key, string[]? values = null) {
            Prevent.NullEmptyOrWhiteSpace(key, nameof(key));

            Key = key;
            Values = values ?? Array.Empty<string>();
        }

        #endregion

        #region Public Methods

        public bool Equals(Translation? other) => other != null && other.Key == Key;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as Translation);

        public override int GetHashCode() => (Key ?? string.Empty).GetHashCode();

        #endregion
    }
}
