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
        private readonly IDistributedCacheAdapter _distributedCache;

        public DistributedMutexElection(IDistributedCacheAdapter distributedCache)
        {
            if (distributedCache == null) throw new ArgumentNullException(nameof(distributedCache));
            if (distributedCache.CacheType != CacheType.Distributed)
                throw new ConfigurationErrorsException("DistributedElection requires a distributed cache");
            this._distributedCache = distributedCache;
        }

        public async Task<T> ExecuteIfLeader<T>(string identity, string context, Func<CancellationToken, Task<T>> leaderAction)
        {
            return await this._distributedCache.Mutex.TryTakeMutexAndExecuteTask<T>(identity + context, leaderAction, TimeSpan.FromMinutes(1), CancellationToken.None);
        }
    }
}