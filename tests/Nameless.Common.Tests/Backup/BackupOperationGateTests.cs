namespace Nameless.Backup;

[UnitTest]
public class BackupOperationGateTests
{
    [Fact]
    public void Second_enter_fails_while_lease_is_held()
    {
        using var gate = new BackupOperationGate();

        using var first = gate.TryEnter();
        var second = gate.TryEnter();

        Assert.NotNull(first);
        Assert.Null(second);
    }

    [Fact]
    public void Enter_succeeds_after_lease_is_released()
    {
        using var gate = new BackupOperationGate();

        gate.TryEnter()!.Dispose();
        using var again = gate.TryEnter();

        Assert.NotNull(again);
    }

    [Fact]
    public void Disposing_a_lease_twice_releases_once()
    {
        using var gate = new BackupOperationGate();

        var lease = gate.TryEnter()!;
        lease.Dispose();
        lease.Dispose();

        using var held = gate.TryEnter();

        Assert.NotNull(held);
        Assert.Null(gate.TryEnter());
    }
}
