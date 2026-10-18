namespace Nameless.Backup.Internals;

/// <summary>
///     Lets a single backup or restore run at a time. Registered as a
///     singleton.
/// </summary>
internal sealed class BackupOperationGate : IDisposable {
    private readonly SemaphoreSlim _semaphore = new(initialCount: 1, maxCount: 1);

    /// <summary>
    ///     Enters the gate without waiting.
    /// </summary>
    /// <returns>
    ///     A lease that leaves the gate when disposed, or
    ///     <see langword="null"/> if another operation holds the gate.
    /// </returns>
    public IDisposable? TryEnter() {
        return _semaphore.Wait(millisecondsTimeout: 0)
            ? new Lease(_semaphore)
            : null;
    }

    public void Dispose() {
        _semaphore.Dispose();
    }

    private sealed class Lease(SemaphoreSlim semaphore) : IDisposable {
        private int _released;

        public void Dispose() {
            if (Interlocked.Exchange(ref _released, value: 1) == 0) {
                semaphore.Release();
            }
        }
    }
}
