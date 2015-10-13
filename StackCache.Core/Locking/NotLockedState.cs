namespace StackCache.Core.Locking
{
    public class NotLocked : IMutexState
    {
        /// <summary>
        ///  Avoids extra allocation
        /// </summary>
        public static readonly NotLocked State = new NotLocked();

        public void Dispose()
        {
        }

        public bool IsLockAcquired => false;
    }
}