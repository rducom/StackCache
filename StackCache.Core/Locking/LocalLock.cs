namespace StackCache.Core.Locking
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Locks against an indexed semaphoreSlim, and return directly when lock cannot be acquired
    /// </summary>
    public class LocalLock : ILock
    {
        private readonly Dictionary<string, ReferencedSemaphoreSlim> _locks =
            new Dictionary<string, ReferencedSemaphoreSlim>();

        /// <summary>
        /// Try to acquired the local lock.
        /// If the lock is acquired, then ILockState.IsLockAcquired returns true.
        /// </summary>
        /// <param name="key">The key to lock on</param>
        /// <param name="timeout">The maximum lock retention</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Returns and ILockState instance, which MUST be disposed (to release the lock)</returns>
        public async Task<ILockState> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            // TODO : implement concurrent wait for timeout
            // TODO : implement cancellationToken

            ReferencedSemaphoreSlim item;
            lock (this._locks)
            {
                if (this._locks.TryGetValue(key, out item))
                {
                    ++item.ReferenceCount;
                }
                else
                {
                    item = new ReferencedSemaphoreSlim();
                    this._locks[key] = item;
                }
            }
            bool isLockAcquired = await item.WaitAsync(TimeSpan.Zero, cancellationToken);
            return new SemaphoreSlimReleaser(key, isLockAcquired, this._locks);
        }

        sealed class ReferencedSemaphoreSlim : SemaphoreSlim
        {
            public ReferencedSemaphoreSlim()
                : base(1, 1)
            {
            }
            public int ReferenceCount { get; set; }
        }

        sealed class SemaphoreSlimReleaser : ILockState
        {
            private readonly string _key;
            private readonly Dictionary<string, ReferencedSemaphoreSlim> _locks;

            public SemaphoreSlimReleaser(string key, bool isLockAcquired, Dictionary<string, ReferencedSemaphoreSlim> locks)
            {
                this._locks = locks;
                this._key = key;
                this.IsLockAcquired = isLockAcquired;
            }

            public void Dispose()
            {
                lock (this._locks)
                {
                    ReferencedSemaphoreSlim item;
                    if (!this._locks.TryGetValue(this._key, out item))
                        return;
                    --item.ReferenceCount;
                    if (this.IsLockAcquired)
                        item.Release();
                    if (item.ReferenceCount == 0)
                        this._locks.Remove(this._key);
                }
            }

            public bool IsLockAcquired { get; }
        }
    }
}