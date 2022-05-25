namespace Nameless.Lucene {

    public sealed class EmptySearchHit : ISearchHit {

        #region Private Static Read-Only Fields

        private static readonly ISearchHit _instance = new EmptySearchHit();

        #endregion

        #region Public Static Properties

        public static ISearchHit Instance => _instance;

        #endregion

        #region Static Constructors

        // Explicit static constructor to tell the C# compiler
        // not to mark type as beforefieldinit
        static EmptySearchHit() { }

        #endregion

        #region Private Constructors

        private EmptySearchHit() { }

        #endregion

        #region ISearchHit Members

        public string DocumentID => string.Empty;

        public float Score => 0F;

        public bool GetBoolean(string fieldName) => false;

        public DateTimeOffset GetDateTimeOffset(string fieldName) => DateTimeOffset.MinValue;

        public double GetDouble(string fieldName) => double.NaN;

        public int GetInt(string fieldName) => -1;

        public string GetString(string fieldName) => string.Empty;

        #endregion
    }
}
