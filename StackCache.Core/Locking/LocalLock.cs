namespace StackCache.Core.Locking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;

    /// <summary>
    /// Try to take a lock for a given key and returns directly wheter the lock is acquired or not
    /// </summary>
    public class IndexedMutex : IMutex
    {
        private readonly ConcurrentDictionary<CacheKey, object> _lockedIdentities = new ConcurrentDictionary<CacheKey, object>();

        /// <summary>
        /// Try to acquire an indexed local lock.
        /// If the lock is acquired, then ILockState.IsLockAcquired returns true.
        /// If key is already locked, then the Lock() returns directly
        /// </summary>
        public async Task TryTake(CacheKey key, Func<CancellationToken, Task> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var callerIdentity = new object();
            object referencedIdentity = this._lockedIdentities.GetOrAdd(key, cacheKey => callerIdentity);
            if (referencedIdentity != callerIdentity)
                return;
            try
            {
                await task(cancellationToken);
            }
            finally
            {
                this._lockedIdentities.TryRemove(key, out callerIdentity);
            }
        }
    }

    /// <summary>
    /// Locks against an indexed semaphoreSlim, and return directly when lock cannot be acquired
    /// </summary>
    public class IndexedLock
    {
        private readonly Dictionary<string, ReferencedSemaphoreSlim> _locks = new Dictionary<string, ReferencedSemaphoreSlim>();

        /// <summary>
        /// Try to acquired the local lock.
        /// If the lock is acquired, then ILockState.IsLockAcquired returns true.
        /// If key is already locked, then the Lock() returns directly
        /// </summary>
        /// <param name="key">The key to lock on</param>
        /// <param name="timeout">The maximum lock retention TimeSpan</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Returns and ILockState instance, which MUST be disposed (to release the lock)</returns>
        public async Task<IMutexState> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken)
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
            return new SemaphoreSlimReleaser(key, isLockAcquired, this);
        }

        sealed class ReferencedSemaphoreSlim : SemaphoreSlim
        {
            public ReferencedSemaphoreSlim()
                : base(1, 1)
            {
            }
            public int ReferenceCount { get; set; }
        }

        sealed class SemaphoreSlimReleaser : IMutexState
        {
            private readonly string _key;
            private readonly IndexedLock _indexedMutex;

            public SemaphoreSlimReleaser(string key, bool isLockAcquired, IndexedLock indexedMutex)
            {
                this._indexedMutex = indexedMutex;
                this._key = key;
                this.IsLockAcquired = isLockAcquired;
            }

            public void Dispose()
            {
                lock (this._indexedMutex._locks)
                {
                    ReferencedSemaphoreSlim item;
                    if (!this._indexedMutex._locks.TryGetValue(this._key, out item))
                        return;
                    --item.ReferenceCount;
                    if (this.IsLockAcquired)
                        item.Release();
                    if (item.ReferenceCount == 0)
                        this._indexedMutex._locks.Remove(this._key);
                }
            }

            public bool IsLockAcquired { get; }
        }
    }
}