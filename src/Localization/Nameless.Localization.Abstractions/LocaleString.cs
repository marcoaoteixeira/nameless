using System.Globalization;

namespace Nameless.Localization {

    public sealed class LocaleString {

        #region Public Properties

        public CultureInfo Culture { get; }
        public string Text { get; }
        public string Translation { get; }
        public object[] Args { get; }

        #endregion

        #region Public Constructors

        public LocaleString(CultureInfo culture, string text, string? translation = null, params object[] args) {
            Ensure.NotNull(culture, nameof(culture));
            Ensure.NotNullEmptyOrWhiteSpace(text, nameof(text));

            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Translation = translation ?? text ?? throw new ArgumentNullException(nameof(translation));
            Args = args;
        }

        #endregion

        #region Public Methods

        public string GetOriginal() => !Args.IsNullOrEmpty() ? string.Format(Culture, Text, Args) : Text;
        public string GetTranslation() => !Args.IsNullOrEmpty() ? string.Format(Culture, Translation, Args) : Translation;

        public bool Equals(LocaleString? obj) =>
            obj != null &&
            obj.Culture == Culture &&
            obj.Text == Text &&
            obj.Translation == Translation;

        #endregion

        #region Public Explicit Operator

        public static implicit operator string?(LocaleString other) => other?.ToString();

        #endregion

        #region Public Override Methods

        public override bool Equals(object? obj) => Equals(obj as LocaleString);

        public override int GetHashCode() {
            var hash = 13;
            unchecked {
                hash += Culture.GetHashCode() * 7;
                hash += Text.GetHashCode() * 7;
                hash += Translation.GetHashCode() * 7;
            }
            return hash;
        }

        public override string ToString() => GetTranslation();

        #endregion
    }
}