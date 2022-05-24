namespace Nameless.Caching {

    public class CacheEntryOptions {

        #region Private Fields

        private DateTimeOffset? _absoluteExpiration;
        private TimeSpan? _slidingExpiration;

        #endregion

        #region Public Properties

        public DateTimeOffset? AbsoluteExpiration {
            get { return _absoluteExpiration; }
            set {
                if (value < DateTimeOffset.Now) {
                    throw new ArgumentOutOfRangeException(nameof(AbsoluteExpiration), value, "The relative expiration value must be positive.");
                }

                _absoluteExpiration = value;
                _slidingExpiration = null;
            }
        }
        public TimeSpan? SlidingExpiration {
            get { return _slidingExpiration; }
            set {
                if (value <= TimeSpan.Zero) {
                    throw new ArgumentOutOfRangeException(nameof(SlidingExpiration), value, "The sliding expiration value must be positive.");
                }

                _absoluteExpiration = null;
                _slidingExpiration = value;
            }
        }

        #endregion
    }
}