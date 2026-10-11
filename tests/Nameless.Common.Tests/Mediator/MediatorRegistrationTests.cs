using Nameless.Mediator.Requests;
using Nameless.Mediator.Streams;

namespace Nameless.Mediator;

public class PassThroughRequestBehavior<TRequest, TResponse> : IRequestPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> {
    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        return next(cancellationToken);
    }
}

public class PassThroughStreamBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : IStream<TResponse> {
    public IAsyncEnumerable<TResponse> HandleAsync(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken) {
        return next();
    }
}

public class GenericEventHandler<TEvent> : Events.IEventHandler<TEvent> where TEvent : Events.IEvent {
    public Task HandleAsync(TEvent evt, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}

public record ScanStructRequest : IRequest;

// Value type handlers must never be picked up by the assembly scan.
public struct ScanStructRequestHandler : IRequestHandler<ScanStructRequest> {
    public readonly Task HandleAsync(ScanStructRequest request, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}

// Vertical-slice layout: the handler is nested in a public static class.
public static class ScanNestedFeature {
    public record Request : IRequest<int>;

    public sealed class Handler : IRequestHandler<Request, int> {
        public Task<int> HandleAsync(Request request, CancellationToken cancellationToken) {
            return Task.FromResult(1);
        }
    }
}

[UnitTest]
public class MediatorRegistrationTests {
    private static MediatorRegistration CreateSut() {
        return new MediatorRegistration().WithUseAssemblyScan(false);
    }

    [Fact]
    public void WithEventHandler_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        var returned = sut.WithEventHandler<MediatorTestEventHandler, MediatorTestEvent>();

        // assert
        Assert.Multiple(
            () => Assert.Same(sut, returned),
            () => Assert.Contains(typeof(MediatorTestEventHandler), sut.EventHandlers)
        );
    }

    [Fact]
    public void WithEventHandler_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithEventHandler(typeof(string)));
    }

    [Fact]
    public void WithRequestHandler_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithRequestHandler<MediatorTestRequestHandler, MediatorTestRequest, string>();

        // assert
        Assert.Contains(typeof(MediatorTestRequestHandler), sut.RequestHandlers);
    }

    [Fact]
    public void WithRequestHandler_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithRequestHandler(typeof(string)));
    }

    [Fact]
    public void WithStreamHandler_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithStreamHandler<MediatorTestStreamHandler, MediatorTestStream, int>();

        // assert
        Assert.Contains(typeof(MediatorTestStreamHandler), sut.StreamHandlers);
    }

    [Fact]
    public void WithStreamHandler_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithStreamHandler(typeof(string)));
    }

    [Fact]
    public void WithRequestPipelineBehavior_AddsOnce() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithRequestPipelineBehavior(typeof(PassThroughRequestBehavior<,>));
        sut.WithRequestPipelineBehavior(typeof(PassThroughRequestBehavior<,>));

        // assert
        Assert.Single(sut.RequestPipelineBehaviors);
    }

    [Fact]
    public void WithRequestPipelineBehavior_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithRequestPipelineBehavior<PassThroughRequestBehavior<MediatorTestRequest, string>, MediatorTestRequest, string>();

        // assert
        Assert.Contains(typeof(PassThroughRequestBehavior<MediatorTestRequest, string>), sut.RequestPipelineBehaviors);
    }

    [Fact]
    public void WithRequestPipelineBehavior_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithRequestPipelineBehavior(typeof(string)));
    }

    [Fact]
    public void WithStreamPipelineBehavior_AddsOnce() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithStreamPipelineBehavior(typeof(PassThroughStreamBehavior<,>));
        sut.WithStreamPipelineBehavior(typeof(PassThroughStreamBehavior<,>));

        // assert
        Assert.Single(sut.StreamPipelineBehaviors);
    }

    [Fact]
    public void WithStreamPipelineBehavior_Generic_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithStreamPipelineBehavior<PassThroughStreamBehavior<MediatorTestStream, int>, MediatorTestStream, int>();

        // assert
        Assert.Contains(typeof(PassThroughStreamBehavior<MediatorTestStream, int>), sut.StreamPipelineBehaviors);
    }

    [Fact]
    public void WithStreamPipelineBehavior_WithUnrelatedType_Throws() {
        // act & assert
        Assert.Throws<ArgumentException>(() => CreateSut().WithStreamPipelineBehavior(typeof(string)));
    }

    // ── fail fast on types the mediator can never invoke ──────────────────────

    [Fact]
    public void WithRequestHandler_OpenGenericWithMismatchedTypeParameters_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithRequestHandler(typeof(InspWrappedArityHandler<>)));
    }

    [Fact]
    public void WithRequestHandler_VoidHandlerForRequestWithResponse_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithRequestHandler(typeof(InspVoidForResponseHandler)));
    }

    [Fact]
    public void WithRequestHandler_CovariantResponseType_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithRequestHandler(typeof(InspCovariantHandler)));
    }

    [Fact]
    public void WithEventHandler_OpenGenericWithMismatchedTypeParameters_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithEventHandler(typeof(InspWrappedEventHandler<>)));
    }

    [Fact]
    public void WithStreamHandler_CovariantResponseType_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithStreamHandler(typeof(InspCovariantStreamHandler)));
    }

    [Fact]
    public void WithRequestPipelineBehavior_OpenGenericWithMismatchedTypeParameters_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithRequestPipelineBehavior(typeof(InspWrappedArityBehavior<>)));
    }

    [Fact]
    public void WithRequestPipelineBehavior_VoidBehaviorForRequestWithResponse_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithRequestPipelineBehavior(typeof(InspVoidBehaviorForResponse)));
    }

    [Fact]
    public void WithStreamPipelineBehavior_ResponseTypeNotDeclaredByStream_ThrowsInvalidOperationException() {
        // act & assert
        Assert.Throws<InvalidOperationException>(() => CreateSut().WithStreamPipelineBehavior(typeof(InspWrongResponseStreamBehavior)));
    }

    [Fact]
    public void WithRequestHandler_HandlerForSeveralRequests_AddsType() {
        // act
        var sut = CreateSut().WithRequestHandler(typeof(InspMultiHandler));

        // assert
        Assert.Contains(typeof(InspMultiHandler), sut.RequestHandlers);
    }

    // ── assembly scan XOR manual registration ─────────────────────────────────

    [Fact]
    public void Handlers_WithAssemblyScan_IgnoreManualRegistrations() {
        // arrange
        var sut = new MediatorRegistration().WithAssemblyFrom<MediatorRegistrationTests>()
                                            .WithRequestHandler(typeof(InspMultiHandler))
                                            .WithEventHandler(typeof(InspEventHandler))
                                            .WithStreamHandler(typeof(InspMultiStreamHandler));

        // act & assert
        Assert.Multiple(
            () => Assert.DoesNotContain(typeof(InspMultiHandler), sut.RequestHandlers),
            () => Assert.Contains(typeof(MediatorTestRequestHandler), sut.RequestHandlers),
            () => Assert.DoesNotContain(typeof(InspEventHandler), sut.EventHandlers),
            () => Assert.Contains(typeof(MediatorTestEventHandler), sut.EventHandlers),
            () => Assert.DoesNotContain(typeof(InspMultiStreamHandler), sut.StreamHandlers),
            () => Assert.Contains(typeof(MediatorTestStreamHandler), sut.StreamHandlers)
        );
    }

    [Fact]
    public void Handlers_WithoutAssemblyScan_ReturnOnlyManualRegistrations() {
        // arrange
        var sut = CreateSut().WithAssemblyFrom<MediatorRegistrationTests>()
                             .WithRequestHandler(typeof(InspMultiHandler))
                             .WithEventHandler(typeof(InspEventHandler))
                             .WithStreamHandler(typeof(InspMultiStreamHandler));

        // act & assert
        Assert.Multiple(
            () => Assert.Equal([typeof(InspMultiHandler)], sut.RequestHandlers),
            () => Assert.Equal([typeof(InspEventHandler)], sut.EventHandlers),
            () => Assert.Equal([typeof(InspMultiStreamHandler)], sut.StreamHandlers)
        );
    }

    [Fact]
    public void RequestHandlers_WithAssemblyScan_FindsNestedPublicHandlers() {
        // arrange
        var sut = new MediatorRegistration().WithAssemblyFrom<MediatorRegistrationTests>();

        // act & assert
        Assert.Contains(typeof(ScanNestedFeature.Handler), sut.RequestHandlers);
    }

    [Fact]
    public void RequestHandlers_WithAssemblyScan_ExcludesValueTypes() {
        // arrange
        var sut = new MediatorRegistration().WithAssemblyFrom<MediatorRegistrationTests>();

        // act & assert
        Assert.DoesNotContain(typeof(ScanStructRequestHandler), sut.RequestHandlers);
    }
}
