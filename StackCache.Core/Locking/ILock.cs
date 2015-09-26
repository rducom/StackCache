namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Generic locking against a shared distributed mutex
    /// </summary>
    public interface ILock
    {
        /// <summary>
        /// Try to acquire a distributed lock at a given key, for the duration defined by @timeout
        /// If the lock is acquired, then ILockState.IsLockAcquired returns true.
        /// </summary>
        /// <param name="key">The key to lock on</param>
        /// <param name="timeout">The maximum lock retention</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Returns and ILockState instance, which MUST be disposed (to release the lock)</returns>
        Task<ILockState> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken);
    }

    public interface ILockState : IDisposable
    {
        bool IsLockAcquired { get; }
    }

    public class NotLockedState : ILockState
    {
        public void Dispose()
        {
        }

        public bool IsLockAcquired => false;
    }
}