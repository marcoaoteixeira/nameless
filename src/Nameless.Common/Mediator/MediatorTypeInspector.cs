using Nameless.Mediator.Requests;
using Nameless.Mediator.Streams;

namespace Nameless.Mediator;

/// <summary>
///     Resolves the service types a mediator handler or pipeline behavior
///     must be registered as, rejecting the types that the mediator would
///     never be able to resolve or invoke.
/// </summary>
internal static class MediatorTypeInspector {
    /// <summary>
    ///     Gets the service types that <paramref name="implementation"/>
    ///     must be registered as for <paramref name="openService"/>.
    /// </summary>
    /// <param name="implementation">
    ///     The handler or pipeline behavior type.
    /// </param>
    /// <param name="openService">
    ///     The open generic service definition, e.g.
    ///     <see cref="IRequestHandler{TRequest,TResponse}"/>.
    /// </param>
    /// <returns>
    ///     The open service itself when <paramref name="implementation"/> is
    ///     an open generic; the closed service interfaces it implements
    ///     otherwise. Empty when it does not implement
    ///     <paramref name="openService"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     When <paramref name="implementation"/> is an open generic whose
    ///     type parameters don't map one-to-one onto the service, or when a
    ///     closed service it implements can never be invoked.
    /// </exception>
    internal static Type[] GetServiceTypes(Type implementation, Type openService) {
        var interfaces = implementation.GetInterfaces()
                                       .Where(@interface => @interface.IsGenericType &&
                                                            @interface.GetGenericTypeDefinition() == openService)
                                       .ToArray();

        if (interfaces.Length == 0) { return []; }

        if (implementation.IsGenericTypeDefinition) {
            EnsureOpenGenericShape(implementation, interfaces);

            return [openService];
        }

        foreach (var @interface in interfaces) {
            EnsureInvocable(implementation, @interface);
        }

        return interfaces;
    }

    // The service provider closes an open generic implementation by passing
    // the service type arguments, in order, as the implementation type
    // arguments. Any other shape either crashes the provider build or
    // resolves a type that doesn't implement the requested service.
    private static void EnsureOpenGenericShape(Type implementation, Type[] interfaces) {
        var parameters = implementation.GetGenericArguments();

        foreach (var @interface in interfaces) {
            if (@interface.GetGenericArguments().SequenceEqual(parameters)) { continue; }

            // suggest the declaration the provider can close: the service
            // type parameters, in order, as the implementation ones.
            var definition = @interface.GetGenericTypeDefinition();
            var definitionParameters = string.Join(",", definition.GetGenericArguments().Select(parameter => parameter.Name));
            var expected = $"{implementation.Name.Split('`')[0]}<{definitionParameters}> : {definition.GetPrettyName()}";

            throw new InvalidOperationException(
                $"Open generic type '{implementation.GetPrettyName()}' " +
                $"implements '{@interface.GetPrettyName()}', whose type " +
                $"arguments are not its own type parameters in the same " +
                $"order. Declare it as '{expected}', or register closed " +
                "types instead."
            );
        }
    }

    private static void EnsureInvocable(Type implementation, Type service) {
        var definition = service.GetGenericTypeDefinition();
        var arguments = service.GetGenericArguments();

        if (definition == typeof(IRequestHandler<>)) {
            EnsureRequestWithoutResponse(implementation, service, arguments[0], typeof(IRequestHandler<,>));
        }

        if (definition == typeof(IRequestPipelineBehavior<>)) {
            EnsureRequestWithoutResponse(implementation, service, arguments[0], typeof(IRequestPipelineBehavior<,>));
        }

        if (definition == typeof(IRequestHandler<,>) || definition == typeof(IRequestPipelineBehavior<,>)) {
            EnsureDeclaredResponse(implementation, service, arguments[0], arguments[1], typeof(IRequest<>), exactlyOne: true);
        }

        if (definition == typeof(IStreamHandler<,>) || definition == typeof(IStreamPipelineBehavior<,>)) {
            EnsureDeclaredResponse(implementation, service, arguments[0], arguments[1], typeof(IStream<>), exactlyOne: false);
        }
    }

    // A request that declares a response is always dispatched through the
    // service that carries the response type, so the void one never runs.
    private static void EnsureRequestWithoutResponse(Type implementation, Type service, Type request, Type origin) {
        var responses = GetDeclaredResponses(request, typeof(IRequest<>));

        if (responses.Length == 0) { return; }

        var implementationName = implementation.GetPrettyName();
        var serviceName = service.GetPrettyName();
        var requestName = request.GetPrettyName();
        var originName = origin.MakeGenericType(request, responses[0]).GetPrettyName();

        throw new InvalidOperationException(
            $"Type '{implementationName}' implements '{serviceName}', which " +
            $"is never invoked: '{requestName}' declares a response, so it " +
            $"is always dispatched with it. Implement '{originName}' instead."
        );
    }

    // The invoker resolves the service closed over the response type the
    // request declares; a different (e.g. covariant) response type is never
    // resolved.
    private static void EnsureDeclaredResponse(Type implementation, Type service, Type request, Type response, Type contract, bool exactlyOne) {
        var responses = GetDeclaredResponses(request, contract);

        var valid = exactlyOne
            ? responses is [var declared] && declared == response
            : responses.Contains(response);

        if (valid) { return; }

        var expected = contract.MakeGenericType(response).GetPrettyName();
        var actual = responses.Length == 0
            ? "none"
            : string.Join(", ", responses.Select(item => $"'{contract.MakeGenericType(item).GetPrettyName()}'"));

        var implementationName = implementation.GetPrettyName();
        var serviceName = service.GetPrettyName();
        var requestName = request.GetPrettyName();

        throw new InvalidOperationException(
            $"Type '{implementationName}' implements '{serviceName}', which " +
            $"is never invoked: '{requestName}' must declare " +
            $"{(exactlyOne ? "exactly " : string.Empty)}'{expected}', but " +
            $"declares {actual}."
        );
    }

    private static Type[] GetDeclaredResponses(Type request, Type contract) {
        return [
            .. request.GetInterfaces()
                      .Where(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == contract)
                      .Select(@interface => @interface.GetGenericArguments()[0])
        ];
    }
}
