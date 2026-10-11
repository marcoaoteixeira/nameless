using Microsoft.Extensions.DependencyInjection;
using Nameless.Mediator.Requests;

namespace Nameless.Mediator;

public sealed class OpenGenericVoidHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest {
    public Task HandleAsync(TRequest request, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}

public sealed class OpenGenericTypedHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse> {
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken) {
        return Task.FromResult(default(TResponse)!);
    }
}

[UnitTest]
public class VoidRequestRegistrationTests {
    private static MediatorRegistration CreateSut() {
        return new MediatorRegistration().WithUseAssemblyScan(false);
    }

    // ── MediatorRegistration ──────────────────────────────────────────────────

    [Fact]
    public void WithRequestHandler_ForVoidHandler_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        var returned = sut.WithRequestHandler<VoidRequestHandler, VoidRequest>();

        // assert
        Assert.Multiple(
            () => Assert.Same(sut, returned),
            () => Assert.Contains(typeof(VoidRequestHandler), sut.RequestHandlers)
        );
    }

    [Fact]
    public void WithRequestHandler_Type_AcceptsVoidHandler() {
        // act
        var sut = CreateSut().WithRequestHandler(typeof(VoidRequestHandler));

        // assert
        Assert.Contains(typeof(VoidRequestHandler), sut.RequestHandlers);
    }

    [Fact]
    public void WithRequestPipelineBehavior_ForVoidBehavior_AddsType() {
        // arrange
        var sut = CreateSut();

        // act
        sut.WithRequestPipelineBehavior<VoidBehaviorA<VoidRequest>, VoidRequest>();

        // assert
        Assert.Contains(typeof(VoidBehaviorA<VoidRequest>), sut.RequestPipelineBehaviors);
    }

    [Fact]
    public void WithRequestPipelineBehavior_Type_AcceptsOpenGenericVoidBehavior() {
        // act
        var sut = CreateSut().WithRequestPipelineBehavior(typeof(VoidBehaviorA<>));

        // assert
        Assert.Contains(typeof(VoidBehaviorA<>), sut.RequestPipelineBehaviors);
    }

    [Fact]
    public void RequestHandlers_WithAssemblyScan_FindsTypedAndVoidHandlers() {
        // arrange
        var sut = new MediatorRegistration().WithAssemblyFrom<VoidRequestHandler>();

        // act
        var handlers = sut.RequestHandlers;

        // assert
        Assert.Multiple(
            () => Assert.Contains(typeof(VoidRequestHandler), handlers),
            () => Assert.Contains(typeof(MediatorTestRequestHandler), handlers),
            () => Assert.Contains(typeof(OpenGenericVoidHandler<>), handlers)
        );
    }

    // ── RegisterMediator ──────────────────────────────────────────────────────

    [Fact]
    public void RegisterMediator_WithClosedVoidHandler_RegistersUnderVoidHandlerInterface() {
        // arrange
        var services = new ServiceCollection();

        // act
        services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithRequestHandler<VoidRequestHandler, VoidRequest>());

        // assert
        Assert.Contains(services, d => d.ServiceType == typeof(IRequestHandler<VoidRequest>) && d.ImplementationType == typeof(VoidRequestHandler));
    }

    [Fact]
    public void RegisterMediator_WithTypedAndVoidHandlers_RegistersEachUnderItsOwnInterface() {
        // arrange
        var services = new ServiceCollection();

        // act
        services.RegisterMediator(r => r
            .WithUseAssemblyScan(false)
            .WithRequestHandler<VoidRequestHandler, VoidRequest>()
            .WithRequestHandler<MediatorTestRequestHandler, MediatorTestRequest, string>());

        // assert
        Assert.Multiple(
            () => Assert.Contains(services, d => d.ServiceType == typeof(IRequestHandler<VoidRequest>)),
            () => Assert.Contains(services, d => d.ServiceType == typeof(IRequestHandler<MediatorTestRequest, string>)),
            () => Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRequestHandler<MediatorTestRequest>))
        );
    }

    [Fact]
    public void RegisterMediator_WithOpenGenericHandlers_RegistersOnlyUnderTheDefinitionTheyImplement() {
        // arrange
        var services = new ServiceCollection();

        // act
        services.RegisterMediator(r => r
            .WithUseAssemblyScan(false)
            .WithRequestHandler(typeof(OpenGenericVoidHandler<>))
            .WithRequestHandler(typeof(OpenGenericTypedHandler<,>)));

        // assert
        Assert.Multiple(
            () => Assert.Contains(services, d => d.ServiceType == typeof(IRequestHandler<>) && d.ImplementationType == typeof(OpenGenericVoidHandler<>)),
            () => Assert.Contains(services, d => d.ServiceType == typeof(IRequestHandler<,>) && d.ImplementationType == typeof(OpenGenericTypedHandler<,>)),
            () => Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRequestHandler<>) && d.ImplementationType == typeof(OpenGenericTypedHandler<,>)),
            () => Assert.DoesNotContain(services, d => d.ServiceType == typeof(IRequestHandler<,>) && d.ImplementationType == typeof(OpenGenericVoidHandler<>))
        );
    }

    [Fact]
    public void RegisterMediator_WithVoidBehavior_RegistersUnderVoidBehaviorInterface() {
        // arrange
        var services = new ServiceCollection();

        // act
        services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithRequestPipelineBehavior(typeof(VoidBehaviorA<>)));

        // assert
        Assert.Contains(services, d => d.ServiceType == typeof(IRequestPipelineBehavior<>) && d.ImplementationType == typeof(VoidBehaviorA<>));
    }
}
