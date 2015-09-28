namespace StackCache.Core.Election
{
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using Locking;

    /// <summary>
    /// Custom leader election based on distributed mutex
    /// </summary>
    public class DistributedMutexElection : IElection
    {
        private readonly ICacheAdapter _distributedCache;

        public DistributedMutexElection(ICacheAdapter distributedCache)
        {
            if (distributedCache == null) throw new ArgumentNullException(nameof(distributedCache));
            if (distributedCache.CacheType != CacheType.Distributed)
                throw new ConfigurationErrorsException("DistributedElection requires a distributed cache");
            this._distributedCache = distributedCache;
        }

        public async Task<T> ExecuteIfLeader<T>(string identity, string context, Func<Task<T>> leaderAction)
        {
            ILock locker = this._distributedCache.GetLocker();
            using (
                ILockState state =
                    await locker.Lock(identity + context, TimeSpan.FromMinutes(1), CancellationToken.None))
            {
                if (state.IsLockAcquired)
                    return await leaderAction();
            }
            return default(T);
        }
    }
}