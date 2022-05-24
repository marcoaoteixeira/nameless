namespace Nameless.Caching.Redis {

    public class CacheOptions {

        #region Public Properties

        public string[] Servers { get; set; } = new[] { "localhost" };

        #endregion
    }
}