namespace Blocks.Core;

public class AsyncLock : IDisposable
{
    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    public async Task<AsyncLock> LockAsync()
    {
        await _semaphoreSlim.WaitAsync();
        return this;
    }

    public AsyncLock Lock()
    {
        _semaphoreSlim.Wait();
        return this;
    }

    public void Dispose()
    {
        _semaphoreSlim.Release();
    }
}