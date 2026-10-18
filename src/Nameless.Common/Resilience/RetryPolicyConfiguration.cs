namespace Nameless.Resilience;

/// <summary>
///     Retry configuration
/// </summary>
public record RetryPolicyConfiguration {
    /// <summary>
    ///     Gets the tag.
    /// </summary>
    public string? Tag { get; init; }

    /// <summary>
    ///     Gets the maximum number of retries.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    ///     Gets the retry initial delay.
    /// </summary>
    public TimeSpan InitialDelay { get; init; }

    /// <summary>
    ///     Gets the backoff type.
    /// </summary>
    public BackoffType BackoffType { get; init; }

    /// <summary>
    ///     Gets the maximum delay between retries.
    /// </summary>
    public TimeSpan MaxDelay { get; init; }

    /// <summary>
    ///     Whether it should use jitter for delay.
    /// </summary>
    public bool UseJitter { get; init; }

    /// <summary>
    ///     Gets a predicate to determine if it should retry on
    ///     a particular exception.
    /// </summary>
    public Func<Exception, bool>? RetryOnException { get; init; }

    /// <summary>
    ///     Gets a callback that will get called before all retries
    ///     attempts.
    /// </summary>
    public Action<Exception?, TimeSpan, int, int>? OnRetry { get; init; }

    /// <summary>
    ///     Gets a predefined retry policy configuration.
    /// </summary>
    /// <param name="onRetry">
    ///     The retry delegate.
    /// </param>
    /// <returns>
    ///     A new instance of <see cref="RetryPolicyConfiguration"/>.
    /// </returns>
    /// <remarks>
    ///     The default parameters are:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>Tag</term>
    ///             <description>
    ///                 <c>Guid.CreateVersion7()</c>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>RetryCount</term>
    ///             <description>
    ///                 <c>3</c>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>InitialDelay</term>
    ///             <description>
    ///                 <c>250ms</c>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>BackoffType</term>
    ///             <description>
    ///                 <c>Exponential</c>
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>UseJitter</term>
    ///             <description>
    ///                 <c>true</c>
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public static RetryPolicyConfiguration CreateDefault(Action<Exception?, TimeSpan, int, int>? onRetry = null) {
        return new RetryPolicyConfiguration {
            Tag = $"{Guid.CreateVersion7():N}",
            RetryCount = 3,
            InitialDelay = TimeSpan.FromMilliseconds(250),
            BackoffType = BackoffType.Exponential,
            MaxDelay = TimeSpan.FromSeconds(5),
            UseJitter = true,
            OnRetry = onRetry
        };
    }
}