using Microsoft.Extensions.DependencyInjection;
using Nameless.Mediator.Events;
using Nameless.Mediator.Requests;
using Nameless.Mediator.Streams;

namespace Nameless.Mediator;

[UnitTest]
public class ServiceCollectionExtensionsTests {
    [Fact]
    public void RegisterMediator_RegistersMediatorAndInvokers() {
        // arrange
        var services = new ServiceCollection();

        // act
        var returned = services.RegisterMediator(r => r.WithUseAssemblyScan(false));
        using var provider = services.BuildServiceProvider();

        // assert
        Assert.Multiple(
            () => Assert.Same(services, returned),
            () => Assert.IsType<MediatorImpl>(provider.GetRequiredService<IMediator>()),
            () => Assert.IsType<EventHandlerInvoker>(provider.GetRequiredService<IEventHandlerInvoker>()),
            () => Assert.IsType<RequestHandlerInvoker>(provider.GetRequiredService<IRequestHandlerInvoker>()),
            () => Assert.IsType<StreamHandlerInvoker>(provider.GetRequiredService<IStreamHandlerInvoker>())
        );
    }

    [Fact]
    public async Task RegisterMediator_WithHandlers_DispatchesEndToEnd() {
        // arrange
        var services = new ServiceCollection();
        services.RegisterMediator(r => r
            .WithUseAssemblyScan(false)
            .WithRequestHandler<MediatorTestRequestHandler, MediatorTestRequest, string>()
            .WithEventHandler<MediatorTestEventHandler, MediatorTestEvent>()
            .WithStreamHandler<MediatorTestStreamHandler, MediatorTestStream, int>()
            .WithRequestPipelineBehavior(typeof(PassThroughRequestBehavior<,>))
            .WithStreamPipelineBehavior(typeof(PassThroughStreamBehavior<,>)));

        using var provider = services.BuildServiceProvider();

        // act
        var response = await provider.GetRequiredService<IRequestHandlerInvoker>()
            .ExecuteAsync(new MediatorTestRequest(), TestContext.Current.CancellationToken);

        var items = new List<int>();
        await foreach (var item in provider.GetRequiredService<IStreamHandlerInvoker>()
                           .CreateAsync(new MediatorTestStream(), TestContext.Current.CancellationToken)) {
            items.Add(item);
        }

        // assert
        Assert.Multiple(
            () => Assert.NotNull(response),
            () => Assert.NotEmpty(items)
        );
    }

    [Fact]
    public void RegisterMediator_WithOpenGenericEventHandler_RegistersOpenGenericDescriptor() {
        // arrange
        var services = new ServiceCollection();

        // act
        services.RegisterMediator(r => r.WithUseAssemblyScan(false).WithEventHandler(typeof(GenericEventHandler<>)));

        // assert
        Assert.Contains(services, d => d.ServiceType == typeof(IEventHandler<>) && d.ImplementationType == typeof(GenericEventHandler<>));
    }

    [Fact]
    public void RegisterMediator_WithoutConfigure_DoesNotThrow() {
        // arrange
        var services = new ServiceCollection();

        // act
        var exception = Record.Exception(() => services.RegisterMediator());

        // assert
        Assert.Null(exception);
    }

    [Fact]
    public void RegisterMediator_CalledTwice_KeepsSingleMediatorRegistration() {
        // arrange
        var services = new ServiceCollection();

        // act
        services.RegisterMediator(r => r.WithUseAssemblyScan(false));
        services.RegisterMediator(r => r.WithUseAssemblyScan(false));

        // assert
        Assert.Single(services, d => d.ServiceType == typeof(IMediator));
    }
}
