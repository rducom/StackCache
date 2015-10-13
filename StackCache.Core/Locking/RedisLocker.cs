namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CacheKeys;
    using StackExchange.Redis;

    public class RedisMutex : IMutexAsync
    {
        private readonly IDatabase _database;

        public RedisMutex(IDatabase database)
        {
            this._database = database;
        }

        public async Task<T> TryTakeMutexAndExecuteTask<T>(CacheKey key, Func<CancellationToken, Task<T>> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            RedisKey redisKey = key;
            RedisValue token = Environment.MachineName + Guid.NewGuid();
            if (!this._database.LockTake(redisKey, token, timeout))
                return default(T);
            T result;
            try
            {
                result = await task(cancellationToken);
            }
            finally
            {
                this._database.LockRelease(redisKey, token);
            }
            return result;
        }
    }
}