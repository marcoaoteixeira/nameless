namespace Nameless.WebApplication.Web.Security {

    public sealed class JwtOptions {

        #region Public Static Read-Only Fields

        public static readonly JwtOptions Default = new() {
            Secret = "zE6o2oXPCPU5xfFj4PGhoa2Y2K+aKsp3ySswAGYz10vpjM0+VnFnEqLUIxsNqW2PG3tvkfdwoNMcH6XHc+N8FQ==",
            RefreshTokenTTL = 60 * 60
        };

        #endregion

        #region Public Properties

        public string? Secret { get; set; }

        /// <summary>
        /// Gets or sets the refresh token time-to-live.
        /// Must be specified in seconds!
        /// </summary>
        public int RefreshTokenTTL { get; set; }

        #endregion
    }
}
