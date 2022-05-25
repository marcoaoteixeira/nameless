using System.Collections;
using System.Dynamic;
using NHibernate.Transform;

namespace Nameless.NHibernate {


    /// <summary>
    /// Singleton Pattern implementation for <see cref="DynamicResultTransformer" />. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    public sealed class DynamicResultTransformer : IResultTransformer {

        #region Private Static Read-Only Fields

        private static readonly IResultTransformer _instance = new DynamicResultTransformer();

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of <see cref="DynamicResultTransformer" />.
        /// </summary>
        public static IResultTransformer Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static DynamicResultTransformer() { }

        #endregion

        #region Private Constructors

        private DynamicResultTransformer() { }

        #endregion

        #region IResultTransformer Members

        public IList TransformList(IList collection) => collection;

        public object TransformTuple(object[] tuple, string[] aliases) {
            Prevent.Null(tuple, nameof(tuple));
            Prevent.Null(aliases, nameof(aliases));

            var result = new ExpandoObject() as IDictionary<string, object>;
            tuple.Each((current, idx) => {
                if (aliases.TryGetByIndex(idx, out var alias)) {
                    if (alias != null) {
                        result[alias] = current;
                    }
                }
            });
            return result;
        }

        #endregion
    }
}
