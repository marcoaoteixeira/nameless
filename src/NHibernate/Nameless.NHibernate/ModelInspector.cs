using NHibernate.Mapping.ByCode;

namespace Nameless.NHibernate {

    /// <summary>
    /// Default implementation of <see cref="ExplicitlyDeclaredModel" />.
    /// </summary>
    public sealed class ModelInspector : ExplicitlyDeclaredModel {

        #region Private Read-Only Fields

        private readonly Type[] _entityTypes;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ModelInspector" />
        /// </summary>
        public ModelInspector(Type[] entityTypes) {
            _entityTypes = entityTypes ?? throw new ArgumentNullException(nameof(entityTypes));
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override bool IsEntity(Type type) {
            return _entityTypes.Any(_ =>
                _.IsGenericType ? type.IsAssignableToGenericType(_) : _.IsAssignableFrom(type)
            );
        }

        #endregion
    }
}
