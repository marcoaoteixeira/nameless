using System.Globalization;

namespace Nameless.Localization {

    public interface ICultureContext {

        #region Methods

        CultureInfo GetCulture();

        #endregion
    }
}
