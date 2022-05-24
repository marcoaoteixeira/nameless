using System.ComponentModel;
using System.Reflection;

namespace Nameless {

    public static class PropertyInfoExtension {

        #region Public Static Methods

        public static string? GetDescription(this PropertyInfo self) {
            if (self == null) { return null; }

            var attr = self.GetCustomAttribute<DescriptionAttribute>(inherit: false);
            return attr?.Description;
        }

        #endregion
    }
}
