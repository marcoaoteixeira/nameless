using System.Globalization;

namespace Nameless.Helpers {

    /// <summary>
    /// <see cref="CultureInfo"/> helpers.
    /// </summary>
    public static class CultureInfoHelper {

        #region Public Static Methods

        /// <summary>
        /// Tries retrieve the <see cref="CultureInfo"/> instance by its name.
        /// </summary>
        /// <param name="cultureName">The culture name</param>
        /// <param name="culture">The output</param>
        /// <param name="defaultCultureName"></param>
        /// <returns><c>true</c> if could retrieve; otherwise <c>false</c>.</returns>
        public static bool TryGetCultureInfo(string cultureName, out CultureInfo culture, string? defaultCultureName = null) {
            try {
                culture = CultureInfo.GetCultureInfo(cultureName);
                return true;
            } catch (CultureNotFoundException) {
                if (!string.IsNullOrWhiteSpace(defaultCultureName)) {
                    try { culture = CultureInfo.GetCultureInfo(defaultCultureName); }
                    catch (CultureNotFoundException) { culture = CultureInfo.CurrentCulture; }
                } else { culture = CultureInfo.CurrentCulture; }
            }
            return false;
        }

        #endregion
    }
}