using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nameless.Localization.Json.Schema {

    public sealed class TranslationGroup {

        #region Private Read-Only Fields

        private readonly Dictionary<string, TranslationCollection> _dictionary;

        #endregion

        #region Public Properties

        public CultureInfo Culture { get; }
        public TranslationCollection[] Values => _dictionary.Values.ToArray();

        #endregion

        #region Public Constructors

        public TranslationGroup(CultureInfo culture, IEnumerable<TranslationCollection>? values = null) {
            Ensure.NotNull(culture, nameof(culture));

            Culture = culture;
            _dictionary = (values ?? Enumerable.Empty<TranslationCollection>()).ToDictionary(_ => _.Key, _ => _);
        }

        #endregion

        #region Public Methods

        public bool TryGetValue(string key, out TranslationCollection? output) => _dictionary.TryGetValue(key, out output);

        public bool Equals(TranslationGroup? other) => other != null && other.Culture?.Name == Culture?.Name;

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as TranslationGroup);

        public override int GetHashCode() => (Culture?.Name ?? string.Empty).GetHashCode();

        #endregion
    }
}
