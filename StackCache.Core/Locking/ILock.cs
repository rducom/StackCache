namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;

    /// <summary>
    /// Lightweight mutual exclusion against a key
    /// </summary>
    public interface IMutex
    {
        /// <summary>
        /// Try to acquire a distributed lock at a given key, for the duration defined by @timeout
        /// If the lock is acquired, then ILockState.IsLockAcquired returns true.
        /// </summary>
        /// <param name="key">The key to lock on</param>
        /// <param name="task"></param>
        /// <param name="timeout">The maximum lock retention</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Returns and ILockState instance, which MUST be disposed (to release the lock)</returns>
        Task TryTake(CacheKey key, Func<CancellationToken, Task> task, TimeSpan timeout, CancellationToken cancellationToken);
    }


    /// <summary>
    /// Lightweight mutual exclusion against a key
    /// </summary>
    public interface IMutexAsync
    {
        /// <summary>
        /// Try to acquire a distributed lock at a given key, for the duration defined by @timeout
        /// If the lock is acquired, then ILockState.IsLockAcquired returns true.
        /// </summary>
        /// <param name="key">The key to lock on</param>
        /// <param name="task">The task to execute if mutex is acquired</param>
        /// <param name="timeout">The maximum lock retention</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Returns and ILockState instance, which MUST be disposed (to release the lock)</returns>
        Task<T> TryTakeMutexAndExecuteTask<T>(CacheKey key, Func<CancellationToken, Task<T>> task, TimeSpan timeout, CancellationToken cancellationToken);
    }
}