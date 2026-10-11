namespace Nameless.Web;

/// <summary>
///     Constants for easy use.
/// </summary>
public static class WebConstants {
    /// <summary>
    ///     List of policies defined as "default".
    /// </summary>
    public static class Policies {
        public static class Cors {
            public const string AllowEverything = "CP: Allow_Everything";
        }

        public static class OutputCache {
            public const string OneSecond = "OC: 1_Seconds";
            public const string FiveSeconds = "OC: 5_Seconds";
            public const string FifteenSeconds = "OC: 15_Seconds";
            public const string ThirtySeconds = "OC: 30_Seconds";
            public const string OneMinute = "OC: 1_Minute";
        }

        public static class RateLimiter {
            /// <summary>
            ///     A rate limiter policy that has a window of 60 seconds,
            ///     permits up to 10 requests and has 4 window segments.
            /// </summary>
            public const string SlidingWindow = "RL: Sliding_Window_60_Seconds_10_Requests_4_Segments";
        }

        public static class RequestTimeout {
            public const string OneSecond = "RT: 1_Seconds";
            public const string FiveSeconds = "RT: 5_Seconds";
            public const string FifteenSeconds = "RT: 15_Seconds";
            public const string ThirtySeconds = "RT: 30_Seconds";
            public const string OneMinute = "RT: 1_Minute";
        }
    }

    /// <summary>
    ///     A collection of content types.
    /// </summary>
    public static class ContentTypes {
        /// <summary>
        ///     Default content type text plain.
        /// </summary>
        public const string Text = "text/plain; charset=utf-8";

        /// <summary>
        ///     JSON content type.
        /// </summary>
        public const string Json = "application/json";

        /// <summary>
        ///     JSON content type with charset UTF-8.
        /// </summary>
        public const string JsonWithCharset = "application/json; charset=utf-8";

        /// <summary>
        ///     Problem content type.
        /// </summary>
        public const string ProblemJson = "application/problem+json";

        /// <summary>
        ///     Binary content type.
        /// </summary>
        public const string Binary = "application/octet-stream";
    }

    /// <summary>
    ///     A collection of constants to use as SyntaxAttribute syntax.
    /// </summary>
    public static class Syntaxes {
        /// <summary>
        ///     Route syntax.
        /// </summary>
        public const string Route = "Route";
    }
}