namespace StackCache.Core.Locking
{
    public class NotLockedState : ILockState
    {
        public void Dispose()
        {
        }

        public bool IsLockAcquired => false;
    }
}