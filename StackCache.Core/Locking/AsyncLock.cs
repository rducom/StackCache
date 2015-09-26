namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class AsyncLock : IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly Task<IDisposable> _releaser;

        public AsyncLock()
        {
            this._releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            Task wait = this._semaphore.WaitAsync();

            if (wait.IsCompleted)
                return this._releaser;

            return wait.ContinueWith((_, state) => (IDisposable)state, this._releaser.Result, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;
            internal Releaser(AsyncLock toRelease)
            {
                this._toRelease = toRelease;
            }
            public void Dispose()
            {
                this._toRelease._semaphore.Release();
            }
        }

        public void Dispose()
        {
            this._semaphore.Dispose();
        }
    }
}