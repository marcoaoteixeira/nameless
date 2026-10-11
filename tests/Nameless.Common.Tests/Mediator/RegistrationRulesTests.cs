using Microsoft.Extensions.DependencyInjection;
using Nameless.Mediator.Events;
using Nameless.Mediator.Requests;
using Nameless.Mediator.Streams;

namespace Nameless.Mediator;

// Fixtures are internal on purpose: duplicates must never be picked up by the
// assembly scan tests that scan this test assembly.

internal record RegEvent : IEvent;

internal sealed class RegEventHandlerA(RequestLog log) : IEventHandler<RegEvent> {
    public Task HandleAsync(RegEvent evt, CancellationToken cancellationToken) {
        log.Add("A");

        return Task.CompletedTask;
    }
}

internal sealed class RegEventHandlerB(RequestLog log) : IEventHandler<RegEvent> {
    public Task HandleAsync(RegEvent evt, CancellationToken cancellationToken) {
        log.Add("B");

        return Task.CompletedTask;
    }
}

internal record RegRequest : IRequest<string>;

internal sealed class RegRequestHandlerA : IRequestHandler<RegRequest, string> {
    public Task<string> HandleAsync(RegRequest request, CancellationToken cancellationToken) => Task.FromResult("A");
}

internal sealed class RegRequestHandlerB : IRequestHandler<RegRequest, string> {
    public Task<string> HandleAsync(RegRequest request, CancellationToken cancellationToken) => Task.FromResult("B");
}

internal sealed class RegOpenRequestHandlerA<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse> {
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken) => Task.FromResult(default(TResponse)!);
}

internal sealed class RegOpenRequestHandlerB<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse> {
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken) => Task.FromResult(default(TResponse)!);
}

internal record RegStream : IStream<int>;

internal sealed class RegStreamHandlerA : IStreamHandler<RegStream, int> {
    public IAsyncEnumerable<int> HandleAsync(RegStream request, CancellationToken cancellationToken) => AsyncEnumerable.Empty<int>();
}

internal sealed class RegStreamHandlerB : IStreamHandler<RegStream, int> {
    public IAsyncEnumerable<int> HandleAsync(RegStream request, CancellationToken cancellationToken) => AsyncEnumerable.Empty<int>();
}

[UnitTest]
public class RegistrationRulesTests {
    private static ServiceCollection CreateServices() {
        var services = new ServiceCollection();
        services.AddSingleton<RequestLog>();

        return services;
    }

    // ── events fan out ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterMediator_TwoClosedHandlersForSameEvent_RunsBoth() {
        // arrange
        var services = CreateServices();
        services.RegisterMediator(r => r.WithUseAssemblyScan(false)
                                        .WithEventHandler(typeof(RegEventHandlerA))
                                        .WithEventHandler(typeof(RegEventHandlerB)));
        using var provider = services.BuildServiceProvider();

        // act
        await provider.GetRequiredService<IMediator>().PublishAsync(new RegEvent(), TestContext.Current.CancellationToken);

        // assert
        Assert.Equivalent(new[] { "A", "B" }, provider.GetRequiredService<RequestLog>().Entries, strict: true);
    }

    [Fact]
    public void RegisterMediator_CalledTwiceWithSameEventHandler_RegistersItOnce() {
        // arrange
        var services = CreateServices();

        // act
        services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithEventHandler(typeof(RegEventHandlerA)));
        services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithEventHandler(typeof(RegEventHandlerA)));

        // assert
        Assert.Single(services, d => d.ServiceType == typeof(IEventHandler<RegEvent>));
    }

    // ── requests and streams have exactly one handler ─────────────────────────

    [Fact]
    public void RegisterMediator_TwoHandlersForSameRequest_ThrowsInvalidOperationExceptionNamingBoth() {
        // arrange
        var services = CreateServices();

        // act
        var exception = Assert.Throws<InvalidOperationException>(
            () => services.RegisterMediator(r => r.WithUseAssemblyScan(false)
                                                  .WithRequestHandler(typeof(RegRequestHandlerA))
                                                  .WithRequestHandler(typeof(RegRequestHandlerB))));

        // assert
        Assert.Multiple(
            () => Assert.Contains(nameof(RegRequestHandlerA), exception.Message),
            () => Assert.Contains(nameof(RegRequestHandlerB), exception.Message)
        );
    }

    [Fact]
    public void RegisterMediator_TwoHandlersForSameStream_ThrowsInvalidOperationException() {
        // arrange
        var services = CreateServices();

        // act & assert
        Assert.Throws<InvalidOperationException>(
            () => services.RegisterMediator(r => r.WithUseAssemblyScan(false)
                                                  .WithStreamHandler(typeof(RegStreamHandlerA))
                                                  .WithStreamHandler(typeof(RegStreamHandlerB))));
    }

    [Fact]
    public void RegisterMediator_DifferentHandlerAlreadyRegisteredForRequest_ThrowsInvalidOperationException() {
        // arrange
        var services = CreateServices();
        services.AddTransient<IRequestHandler<RegRequest, string>, RegRequestHandlerB>();

        // act & assert
        Assert.Throws<InvalidOperationException>(
            () => services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithRequestHandler(typeof(RegRequestHandlerA))));
    }

    [Fact]
    public void RegisterMediator_SameHandlerAlreadyRegisteredForRequest_KeepsSingleRegistration() {
        // arrange
        var services = CreateServices();
        services.AddTransient<IRequestHandler<RegRequest, string>, RegRequestHandlerA>();

        // act
        var exception = Record.Exception(
            () => services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithRequestHandler(typeof(RegRequestHandlerA))));

        // assert
        Assert.Multiple(
            () => Assert.Null(exception),
            () => Assert.Single(services, d => d.ServiceType == typeof(IRequestHandler<RegRequest, string>))
        );
    }

    [Fact]
    public void RegisterMediator_CalledTwiceWithSameHandlers_DoesNotThrow() {
        // arrange
        var services = CreateServices();

        // act
        var exception = Record.Exception(() => {
            for (var index = 0; index < 2; index++) {
                services.RegisterMediator(r => r.WithUseAssemblyScan(false)
                                                .WithRequestHandler(typeof(RegRequestHandlerA))
                                                .WithRequestHandler(typeof(RegOpenRequestHandlerA<,>))
                                                .WithStreamHandler(typeof(RegStreamHandlerA))
                                                .WithEventHandler(typeof(RegEventHandlerA)));
            }
        });

        // assert
        Assert.Null(exception);
    }

    [Fact]
    public void RegisterMediator_TwoOpenGenericHandlersForSameService_ThrowsInvalidOperationException() {
        // arrange
        var services = CreateServices();

        // act & assert
        Assert.Throws<InvalidOperationException>(
            () => services.RegisterMediator(r => r.WithUseAssemblyScan(false)
                                                  .WithRequestHandler(typeof(RegOpenRequestHandlerA<,>))
                                                  .WithRequestHandler(typeof(RegOpenRequestHandlerB<,>))));
    }

    [Fact]
    public async Task RegisterMediator_ClosedAndOpenGenericHandlerForSameRequest_ClosedHandlerWins() {
        // arrange
        var services = CreateServices();
        services.RegisterMediator(r => r.WithUseAssemblyScan(false)
                                        .WithRequestHandler(typeof(RegOpenRequestHandlerA<,>))
                                        .WithRequestHandler(typeof(RegRequestHandlerA)));
        using var provider = services.BuildServiceProvider();

        // act
        var response = await provider.GetRequiredService<IMediator>().ExecuteAsync(new RegRequest(), TestContext.Current.CancellationToken);

        // assert
        Assert.Equal("A", response);
    }

    // ── one class, several requests ───────────────────────────────────────────

    [Fact]
    public async Task RegisterMediator_HandlerForSeveralRequests_DispatchesEachOne() {
        // arrange
        var services = CreateServices();
        services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithRequestHandler(typeof(InspMultiHandler)));
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });
        var mediator = provider.GetRequiredService<IMediator>();

        // act
        var a = await mediator.ExecuteAsync(new InspA(), TestContext.Current.CancellationToken);
        await mediator.ExecuteAsync(new InspB(), TestContext.Current.CancellationToken);
        var c = await mediator.ExecuteAsync(new InspC(), TestContext.Current.CancellationToken);

        // assert
        Assert.Multiple(
            () => Assert.Equal("a", a),
            () => Assert.Equal(1, c)
        );
    }

    // ── types found by the assembly scan ──────────────────────────────────────

    [Fact]
    public void CreateDescriptors_InvalidType_ThrowsInvalidOperationExceptionWithAssemblyScanHint() {
        // arrange
        // manual registration rejects invalid types up front, so an invalid
        // type reaching this point comes from the assembly scan.
        Type[] implementations = [typeof(InspWrappedArityHandler<>)];

        // act
        var exception = Assert.Throws<InvalidOperationException>(
            () => MediatorServiceCollectionExtensions.CreateDescriptors(typeof(IRequestHandler<,>), implementations));

        // assert
        Assert.Multiple(
            () => Assert.Contains("[IgnoreAssemblyScan]", exception.Message),
            () => Assert.IsType<InvalidOperationException>(exception.InnerException),
            () => Assert.StartsWith(exception.InnerException!.Message, exception.Message)
        );
    }
}
