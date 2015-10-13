namespace StackCache.Core.Locking
{
    using System;

    public interface IMutexState : IDisposable
    {
        bool IsLockAcquired { get; }
    }
}