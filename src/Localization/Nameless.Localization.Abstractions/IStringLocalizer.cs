namespace Nameless.Localization {

    public interface IStringLocalizer {

        #region Properties

        LocaleString this[string text, int count = -1, params object[] args] { get; }

        #endregion

        #region Methods

        IEnumerable<LocaleString> List(bool includeParentCultures = false);

        #endregion
    }
}