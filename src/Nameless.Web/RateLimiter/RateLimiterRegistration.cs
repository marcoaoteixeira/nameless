using Microsoft.AspNetCore.RateLimiting;
using Nameless.Registration;

namespace Nameless.Web.RateLimiter;

/// <summary>
///     Provides configuration options for registering and managing rate
///     limiter policies within an application.
/// </summary>
public class RateLimiterRegistration : AssemblyScanAware<RateLimiterRegistration> {
    private readonly Dictionary<string, Type> _rateLimiterPolicies = [];

    /// <summary>
    ///     Gets all registered rate limiter policy types.
    /// </summary>
    public IReadOnlyDictionary<string, Type> RateLimiterPolicies => UseAssemblyScan
        ? ExecuteAssemblyScan(typeof(IRateLimiterPolicy<>)).ToDictionary(
            keySelector: RateLimiterPolicyAttribute.GetName,
            elementSelector: type => type
        )
        : _rateLimiterPolicies;

    /// <summary>
    ///     Registers a rate limiter policy of the specified type, optionally
    ///     assigning a name for identification.
    /// </summary>
    /// <remarks>
    ///     Use this method to register a custom rate limiter policy, enabling
    ///     fine-grained control over request handling based on partition
    ///     keys. Registering multiple policies with different names allows for
    ///     distinct rate limiting strategies within the application.
    /// </remarks>
    /// <typeparam name="TRateLimiterPolicy">
    ///     The type of the rate limiter policy to register. Must implement
    ///     the <see cref="IRateLimiterPolicy{TPartitionKey}"/> interface for
    ///     the specified partition key type.
    /// </typeparam>
    /// <typeparam name="TPartitionKey">
    ///     The type of the partition key used by the rate limiter policy.
    /// </typeparam>
    /// <param name="name">
    ///     The policy name; when <see langword="null"/> it assumes the type
    ///     full name or name.
    /// </param>
    /// <returns>
    ///     The current <see cref="RateLimiterRegistration"/> instance
    ///     so other actions can be chained.
    /// </returns>
    public RateLimiterRegistration WithRateLimiterPolicy<TRateLimiterPolicy, TPartitionKey>(string? name = null)
        where TRateLimiterPolicy : IRateLimiterPolicy<TPartitionKey> {
        return WithRateLimiterPolicy(typeof(TRateLimiterPolicy), name);
    }

    /// <summary>
    ///     Registers a rate limiter policy of the specified type, optionally
    ///     assigning a name for identification.
    /// </summary>
    /// <remarks>
    ///     Use this method to register a custom rate limiter policy, enabling
    ///     fine-grained control over request handling based on partition
    ///     keys. Registering multiple policies with different names allows for
    ///     distinct rate limiting strategies within the application.
    /// </remarks>
    /// <param name="type">
    ///     The type of the rate limiter policy to register. Must implement
    ///     the <see cref="IRateLimiterPolicy{TPartitionKey}"/> interface for
    ///     the specified partition key type.
    /// </param>
    /// <param name="name">
    ///     The policy name; when <see langword="null"/> it assumes the type
    ///     full name or name.
    /// </param>
    /// <returns>
    ///     The current <see cref="RateLimiterRegistration"/> instance
    ///     so other actions can be chained.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     if <paramref name="type"/>
    ///     is an abstract or interface type;
    ///     or is an open-generic type;
    ///     or has no parameterless constructor;
    ///     or is not assignable from generic type <see cref="IRateLimiterPolicy{TPartitionKey}"/>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    public RateLimiterRegistration WithRateLimiterPolicy(Type type, string? name = null) {
        Throws.When.IsNotAssignableFromGeneric(type, typeof(IRateLimiterPolicy<>));
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNonConcreteType(type);
        Throws.When.HasNoParameterlessConstructor(type);

        var innerName = string.IsNullOrWhiteSpace(name)
            ? RateLimiterPolicyAttribute.GetName(type)
            : name;

        _rateLimiterPolicies[innerName] = type;

        return this;
    }
}