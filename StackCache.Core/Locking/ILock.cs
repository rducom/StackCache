namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILock
    {
        Task<IDisposable> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken);
    }
}