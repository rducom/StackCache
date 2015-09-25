namespace StackCache.Core.Locking
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class LocalLock : ILock
    {
        public async Task<IDisposable> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            LockingInstance tolock = NamedLocks.Instance.GetLockingInstance(key);
            if (tolock?.Semaphore == null)
                throw new Exception("Locking exception");
            await tolock.Semaphore.WaitAsync(timeout, cancellationToken);
            return new LockerDispose(tolock.Semaphore);
        }

        class LockerDispose : IDisposable
        {
            private readonly SemaphoreSlim _semaphore;

            public LockerDispose(SemaphoreSlim semaphore)
            {
                if (semaphore == null) throw new ArgumentNullException(nameof(semaphore));
                this._semaphore = semaphore;
            }

            public void Dispose()
            {
                this._semaphore.Release();
            }
        }

        /// <summary>
        /// Thread safe indexed semaphores
        /// </summary>
        class NamedLocks
        {
            static readonly Lazy<NamedLocks> _lazy = new Lazy<NamedLocks>(() => new NamedLocks());
            private NamedLocks()
            {
            }
            public static NamedLocks Instance => _lazy.Value;
            private readonly ConcurrentDictionary<string, LockingInstance> _locks = new ConcurrentDictionary<string, LockingInstance>();
            public LockingInstance GetLockingInstance(string key)
            {
                return this._locks.GetOrAdd(key, (i) => new LockingInstance());
            }
        }
        class LockingInstance : IDisposable
        {
            public readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
            public void Dispose()
            {
                Semaphore.Dispose();
            }
        }
    }
}