namespace StackCache.Core.Locking
{
    using System;
    using System.Threading.Tasks;

    public static class AsyncHelper
    {
        public static AsyncExclusive<T> OnlyOnce<T>(this Func<Task<T>> funk)
        {
            return new AsyncExclusive<T>(funk);
        }
    }
}