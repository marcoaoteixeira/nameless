using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Nameless {

    /// <summary>
    /// <see cref="Enum"/> extension methods.
    /// </summary>
    public static class EnumeratorExtension {

        #region Public Static Methods

        /// <summary>
        /// Retrieves the attributes from a enumator.
        /// </summary>
        /// <param name="self">The self enumerator.</param>
        /// <param name="inherited">Marks as inherited attributes.</param>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <returns>A collection of attributes of type <typeparamref name="TAttribute" /></returns>
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this Enum self, bool inherited = false) where TAttribute : Attribute {
            var field = self.GetType().GetField(self.ToString());

            if (field == null) { return Enumerable.Empty<TAttribute>(); }

            return field.GetCustomAttributes<TAttribute>(inherit: inherited);
        }

        /// <summary>
        /// Gets the enumerator description, if exists.
        /// </summary>
        /// <param name="self">The self enumerator.</param>
        /// <returns>The enumerator description.</returns>
        public static string GetDescription(this Enum self) {
            var attr = self.GetAttributes<DescriptionAttribute>().SingleOrDefault();
            return attr != null ? attr.Description : self.ToString();
        }

        #endregion
    }
}