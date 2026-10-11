using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;

namespace Nameless.Web.RateLimiter;

/// <summary>
///     <see cref="RateLimiterOptions"/> extension methods.
/// </summary>
public static class RateLimiterOptionsExtensions {
    private static readonly MethodInfo AddPolicyHandler = GetAddPolicyHandler();

    /// <param name="self">
    ///     The current instance of <see cref="RateLimiterOptions"/> class.
    /// </param>
    extension(RateLimiterOptions self) {
        /// <summary>
        ///     Registers a rate limiter policy given only its runtime Type.
        /// </summary>
        /// <remarks>
        ///     The type must implement <see cref="IRateLimiterPolicy{TPartitionKey}"/>
        ///     for exactly one TPartitionKey. The concrete policy instance is resolved
        ///     from DI per request, just like the generic <see cref="RateLimiterOptions.AddPolicy{TPartitionKey, TPolicy}"/>.
        /// </remarks>
        /// <param name="name">
        ///     The policy name.
        /// </param>
        /// <param name="type">
        ///     The rate limiter type.
        /// </param>
        /// <returns>
        ///     The current instance of <see cref="RateLimiterOptions"/> so other
        ///     actions can be chained.
        /// </returns>
        public RateLimiterOptions RegisterPolicy(string name, Type type) {
            Throws.When.NullOrWhiteSpace(name);
            Throws.When.Null(type);

            var partitionKey = ExtractPartitionKeyType(type);
            var handler = AddPolicyHandler.MakeGenericMethod(partitionKey, type);

            handler.Invoke(self, [name]);

            return self;
        }
    }

    private static Type ExtractPartitionKeyType(Type policyType) {
        Throws.When.IsNonConcreteType(policyType);

        var matches = policyType
            .GetInterfaces()
            .Where(@interface => @interface.IsGenericType &&
                                 @interface.GetGenericTypeDefinition() == typeof(IRateLimiterPolicy<>))
            .ToArray();

        return matches.Length switch {
            0 => throw new ArgumentException(
                $"'{policyType.FullName}' does not implement IRateLimiterPolicy<TPartitionKey>.", nameof(policyType)
            ),

            1 => matches[0].GetGenericArguments()[0],

            _ => throw new ArgumentException(
                $"'{policyType.FullName}' implements IRateLimiterPolicy<TPartitionKey> more than once. A type must target a single partition key type to be registered this way.", nameof(policyType)
            )
        };
    }

    private static MethodInfo GetAddPolicyHandler() {
        // RateLimiterOptions.AddPolicy<TPartitionKey, TPolicy>(string) is an
        // instance method (not a static extension), so it is looked up
        // directly on RateLimiterOptions and invoked against `self` later.
        var method = typeof(RateLimiterOptions)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(handler =>
                handler is { Name: nameof(RateLimiterOptions.AddPolicy), IsGenericMethodDefinition: true } &&
                handler.GetGenericArguments().Length == 2 &&
                handler.GetParameters().Length == 1 &&
                handler.GetParameters()[0].ParameterType == typeof(string)
            );

        return method ?? throw new InvalidOperationException(
            "Could not locate AddPolicy<TPartitionKey, TPolicy>(string) on RateLimiterOptions. The ASP.NET Core rate limiting API surface may have changed in this SDK version."
        );
    }
}
