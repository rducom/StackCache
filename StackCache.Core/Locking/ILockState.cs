namespace StackCache.Core.Locking
{
    using System;

    public interface ILockState : IDisposable
    {
        bool IsLockAcquired { get; }
    }
}