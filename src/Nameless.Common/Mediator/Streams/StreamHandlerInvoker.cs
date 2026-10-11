using System.Collections.Concurrent;

namespace Nameless.Mediator.Streams;

/// <summary>
///     The default implementation of <see cref="IStreamHandlerInvoker" />.
/// </summary>
internal sealed class StreamHandlerInvoker : IStreamHandlerInvoker {
    // Wrappers are stateless and keyed by type, so they are shared by every
    // invoker instance (the invoker itself is transient). A stream may
    // declare several response types, hence the response type in the key.
    private static readonly ConcurrentDictionary<(Type Request, Type Response), object> Cache = new();

    private readonly IServiceProvider _provider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StreamHandlerInvoker" /> class.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    public StreamHandlerInvoker(IServiceProvider provider) {
        _provider = provider;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     if <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     if the runtime type of <paramref name="request"/> does not declare
    ///     <see cref="IStream{TResponse}"/> for <typeparamref name="TResponse"/>
    ///     (e.g. a stream of <see cref="string"/> requested as a stream of
    ///     <see cref="object"/>).
    /// </exception>
    public IAsyncEnumerable<TResponse> CreateAsync<TResponse>(IStream<TResponse> request, CancellationToken cancellationToken) {
        Throws.When.Null(request);

        var handler = (StreamHandlerWrapper<TResponse>)Cache.GetOrAdd((request.GetType(), typeof(TResponse)), CreateStreamHandlerWrapper);

        return handler.HandleAsync(request, _provider, cancellationToken);
    }

    private static object CreateStreamHandlerWrapper((Type Request, Type Response) key) {
        // IStream<out TResponse> is covariant: a stream declaring string can
        // be passed as IStream<object>, but no handler is closed over object.
        var declared = key.Request.GetTypesThatCloses(typeof(IStream<>))
                                  .Select(type => type.GetGenericArguments()[0])
                                  .ToArray();

        if (!declared.Contains(key.Response)) {
            throw new InvalidOperationException(
                $"Stream '{key.Request.GetPrettyName()}' does not declare '{typeof(IStream<>).MakeGenericType(key.Response).GetPrettyName()}'; " +
                $"create it with one of its declared response types: {string.Join(", ", declared.Select(type => $"'{type.GetPrettyName()}'"))}."
            );
        }

        var wrapperType = typeof(StreamHandlerWrapperImpl<,>).MakeGenericType(key.Request, key.Response);

        return Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException(
            $"Couldn't create stream handler wrapper for stream '{key.Request.GetPrettyName()}'."
        );
    }
}
