using NHibernate.Mapping.ByCode;

namespace Nameless.NHibernate {


    /// <summary>
    /// Singleton Pattern implementation for <see cref="UUIDHexCombGeneratorDef" />. (see: https://en.wikipedia.org/wiki/Singleton_pattern)
    /// </summary>
    public sealed class UUIDHexCombGeneratorDef : IGeneratorDef {

        #region Private Static Read-Only Fields

        private static readonly IGeneratorDef _instance = new UUIDHexCombGeneratorDef("D");

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets the unique instance of <see cref="UUIDHexCombGeneratorDef" />.
        /// </summary>
        public static IGeneratorDef Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static UUIDHexCombGeneratorDef() { }

        #endregion

        #region Private Constructors

        private UUIDHexCombGeneratorDef(string format) {
            Prevent.NullEmptyOrWhiteSpace(format, nameof(format));

            Params = new { format };
        }

        #endregion

        #region IGeneratorDef Members

        /// <inheritdoc />
        public string Class => "uuid.hex";

        /// <inheritdoc />
        public object Params { get; }

        /// <inheritdoc />
        public Type DefaultReturnType => typeof(string);

        /// <inheritdoc />
        public bool SupportedAsCollectionElementId => false;

        #endregion
    }
}
