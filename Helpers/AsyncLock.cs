using System;
using System.Threading;
using System.Threading.Tasks;

namespace TimeClock.Helpers
{
    public class AsyncLock
    {
        private readonly ValueTask<IDisposable> _releaserTask;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IDisposable _releaser;

        public AsyncLock()
        {
            _releaser = new Releaser(_semaphore);
            _releaserTask = new ValueTask<IDisposable>(_releaser);
        }

        public IDisposable Lock()
        {
            _semaphore.Wait();
            return _releaser;
        }

        public async ValueTask<IDisposable> LockAsync()
        {
            await _semaphore.WaitAsync();
            return _releaser;
        }

        private class Releaser : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

            public void Dispose() => _semaphore.Release();
        }
    }
}