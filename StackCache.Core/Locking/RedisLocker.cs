namespace StackCache.Core.Locking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    public class RedisLocker : ILock
    {
        private readonly IDatabase _database;
        private readonly int _milliseconds;
        public RedisLocker(IDatabase database, int retryMilliseconds = 100)
        {
            this._database = database;
            this._milliseconds = retryMilliseconds;
        }

        public async Task<IDisposable> Lock(string key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            RedisKey redisKey = key;
            RedisValue token = Environment.MachineName + Guid.NewGuid();
            do
            {
                if (this._database.LockTake(redisKey, token, timeout))
                    break;
                await Task.Delay(this._milliseconds, cancellationToken);
            } while (true);
            return new RedisLockDispose(this._database, redisKey, token);
        }

        class RedisLockDispose : IDisposable
        {
            private readonly IDatabase _database;
            private readonly RedisKey _key;
            private readonly RedisValue _value;

            public RedisLockDispose(IDatabase database, RedisKey key, RedisValue value)
            {
                if (database == null) throw new ArgumentNullException(nameof(database));
                this._database = database;
                this._key = key;
                this._value = value;
            }

            public void Dispose()
            {
                this._database.LockRelease(this._key, this._value);
            }
        }
    }
}