using NHibernate;

namespace Nameless.NHibernate {

    public static class QueryExtension {

        #region Public Static Methods

        /// <summary>
        /// Converts an <see cref="T:NHibernate.IQuery" /> to a dynamic list.
        /// </summary>
        /// <param name="self">The source <see cref="T:NHibernate.IQuery" />.</param>
        /// <returns>A collection of dynamics, representing the query result.</returns>
        public static IList<dynamic> AsDynamicList(this IQuery self) {
            if (self == null) { return new List<object>(); }

            return self.SetResultTransformer(DynamicResultTransformer.Instance).List<dynamic>();
        }

        #endregion
    }
}
