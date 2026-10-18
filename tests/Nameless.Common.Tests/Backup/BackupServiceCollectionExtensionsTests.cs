using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nameless.Backup.Internals;
using Nameless.Compression;
using Nameless.ObjectModel;

namespace Nameless.Backup;

// Public on purpose: the assembly-scan tests must find them.
public sealed class ScanBackupRoutineA : IBackupRoutine
{
    public string Key => "scan-a";

    public int Order => 0;

    public Task<EmptyResult> BackupAsync(BackupRoutineContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<EmptyResult>(Nothing.Value);
    }

    public Task<EmptyResult> RestoreAsync(RestoreRoutineContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<EmptyResult>(Nothing.Value);
    }
}

public sealed class ScanBackupRoutineB : IBackupRoutine
{
    public string Key => "scan-b";

    public int Order => 1;

    public Task<EmptyResult> BackupAsync(BackupRoutineContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<EmptyResult>(Nothing.Value);
    }

    public Task<EmptyResult> RestoreAsync(RestoreRoutineContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult<EmptyResult>(Nothing.Value);
    }
}

public abstract class AbstractBackupRoutine : IBackupRoutine
{
    public abstract string Key { get; }

    public int Order => 0;

    public abstract Task<EmptyResult> BackupAsync(BackupRoutineContext context, CancellationToken cancellationToken);

    public abstract Task<EmptyResult> RestoreAsync(RestoreRoutineContext context, CancellationToken cancellationToken);
}

[UnitTest]
public class BackupServiceCollectionExtensionsTests
{
    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.RegisterZipCompressor();

        return services;
    }

    private static string[] RoutineKeys(IServiceProvider provider)
    {
        return [.. provider.GetServices<IBackupRoutine>().Select(routine => routine.Key).Order(StringComparer.Ordinal)];
    }

    [Fact]
    public void Manual_registration_registers_only_the_given_routines()
    {
        var services = Services();

        var returned = services.RegisterBackupService(registration => registration.WithUseAssemblyScan(false)
                                                                          .WithRoutine<ScanBackupRoutineA>());

        using var provider = services.BuildServiceProvider();

        Assert.Same(services, returned);
        Assert.Equal(["scan-a"], RoutineKeys(provider));
        Assert.IsType<BackupService>(provider.GetRequiredService<IBackupService>());
    }

    [Fact]
    public void Assembly_scan_finds_public_concrete_routines_and_ignores_manual_ones()
    {
        var services = Services();

        services.RegisterBackupService(registration => registration.WithAssemblyFrom<BackupServiceCollectionExtensionsTests>()
                                                            .WithRoutine<ScanBackupRoutineA>());

        using var provider = services.BuildServiceProvider();

        Assert.Equal(["scan-a", "scan-b"], RoutineKeys(provider));
    }

    [Fact]
    public void Registering_twice_does_not_duplicate_routines()
    {
        var services = Services();

        services.RegisterBackupService(registration => registration.WithUseAssemblyScan(false).WithRoutine<ScanBackupRoutineA>());
        services.RegisterBackupService(registration => registration.WithUseAssemblyScan(false).WithRoutine<ScanBackupRoutineA>());

        using var provider = services.BuildServiceProvider();

        Assert.Equal(["scan-a"], RoutineKeys(provider));
    }

    [Fact]
    public void Service_is_transient_and_gate_is_shared()
    {
        var services = Services();
        services.RegisterBackupService(registration => registration.WithUseAssemblyScan(false).WithRoutine<ScanBackupRoutineA>());

        using var provider = services.BuildServiceProvider();

        Assert.NotSame(provider.GetRequiredService<IBackupService>(), provider.GetRequiredService<IBackupService>());
        Assert.Same(provider.GetRequiredService<BackupOperationGate>(), provider.GetRequiredService<BackupOperationGate>());
        Assert.Same(TimeProvider.System, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void Existing_time_provider_is_kept()
    {
        var services = Services();
        var time = new Microsoft.Extensions.Time.Testing.FakeTimeProvider();
        services.AddSingleton<TimeProvider>(time);

        services.RegisterBackupService();

        using var provider = services.BuildServiceProvider();

        Assert.Same(time, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void Options_are_bound_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Backup:WorkingDirectoryPath"] = "D:/staging",
                ["Backup:CompressionLevel"] = "Fastest"
            })
            .Build();
        var services = Services();

        services.RegisterBackupService(configuration: configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<BackupOptions>>().Value;

        Assert.Equal("D:/staging", options.WorkingDirectoryPath);
        Assert.Equal(System.IO.Compression.CompressionLevel.Fastest, options.CompressionLevel);
    }

    [Fact]
    public void WithRoutine_rejects_abstract_types()
    {
        Assert.Throws<ArgumentException>(() => new BackupRegistration().WithRoutine<AbstractBackupRoutine>());
    }

    [Fact]
    public void WithRoutine_rejects_types_that_are_not_routines()
    {
        Assert.Throws<ArgumentException>(() => new BackupRegistration().WithRoutine(typeof(string)));
    }

    [Fact]
    public void WithRoutine_rejects_open_generic_types()
    {
        Assert.Throws<ArgumentException>(() => new BackupRegistration().WithRoutine(typeof(List<>)));
    }

    [Fact]
    public void WithRoutine_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => new BackupRegistration().WithRoutine(null!));
    }

    [Fact]
    public void Default_scan_finds_no_routine_in_the_library()
    {
        var services = Services();

        services.RegisterBackupService();

        using var provider = services.BuildServiceProvider();

        Assert.Empty(RoutineKeys(provider));
        Assert.NotNull(provider.GetRequiredService<ICompressor>());
    }
}
