namespace StackCache.Core.Distributed
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CacheKeys;
    using Configuration;
    using Locking;
    using Messaging;
    using Serializers;
    using StackExchange.Redis;

    /// <summary>
    /// Redis cache wrapper around StackExchange.Redis
    /// </summary>
    public class RedisCacheAdapter : ICacheAdapter, IMessenger
    {
        public RedisCacheAdapter(IEnumerable<RedisInstance> redisInsances, int databaseNumber, ISerializer serializer)
        {
            List<RedisInstance> instances = redisInsances.ToList();
            this._databaseNumber = databaseNumber;
            this._serializer = serializer;
            string connexion = string.Join(",", instances.Select(i => i.Hostname + ":" + i.Port)); //"server1:6379,server2:6379"
            ConfigurationOptions redisOption = ConfigurationOptions.Parse(connexion);
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisOption);
            RedisInstance master = instances.First();
            this._server = redis.GetServer(master.Hostname, master.Port);
            this._subscriber = redis.GetSubscriber();
            this._db = redis.GetDatabase(databaseNumber);
        }

        private readonly int _databaseNumber;
        private readonly IDatabase _db;
        private readonly ISerializer _serializer;
        private readonly IServer _server;
        private readonly ISubscriber _subscriber;

        public bool Get<T>(CacheKey key, out T value)
        {
            RedisValue rv = this._db.StringGet(key);
            if (rv.IsNullOrEmpty == false)
            {
                value = rv.FromRedisValue<T>(this._serializer);
                return true;
            }
            value = default(T);
            return false;
        }

        public void Put<T>(CacheKey key, T value)
        {
            RedisValue redisValue = value.ToRedisValue(this._serializer);
            this._db.StringSet(key, redisValue);
            switch (key.ExpirationMode)
            {
                case ExpirationMode.Sliding:
                    if (key.ExpirationTimeSpan.HasValue)
                        this._db.KeyExpire(key, key.ExpirationTimeSpan.Value);
                    break;
                case ExpirationMode.AbsoluteUtc:
                    if (key.ExpirationDateTime.HasValue)
                        this._db.KeyExpire(key, key.ExpirationDateTime.Value);
                    break;
            }
        }

        public IEnumerable<T> GetRegion<T>(KeyPrefix prefix)
        {
            IEnumerable<RedisKey> keys = this._server.Keys(this._databaseNumber, prefix.SearchPattern);
            RedisValue[] values = this._db.StringGet(keys.ToArray());
            return values.FromRedisValue<T>(this._serializer);
        }

        public async Task<IEnumerable<T>> GetRegionAsync<T>(KeyPrefix prefix)
        {
            IEnumerable<RedisKey> keys = this._server.Keys(this._databaseNumber, prefix.SearchPattern);
            RedisValue[] values = await this._db.StringGetAsync(keys.ToArray());
            return values.FromRedisValue<T>(this._serializer);
        }

        public async Task<IEnumerable<KeyValuePair<CacheKey, T>>> GetRegionKeyValuesAsync<T>(KeyPrefix prefix)
        {
            // TODO : evaluate performance -> this may be the slower
            IEnumerable<RedisKey> keys = this._server.Keys(this._databaseNumber, prefix.SearchPattern);
            return await Task.WhenAll(
                 keys.Select(async k =>
                 {
                     RedisValue value = await this._db.StringGetAsync(k);
                     return new KeyValuePair<CacheKey, T>(k, value.FromRedisValue<T>(this._serializer));
                 }));
        }

        public void Invalidate(params CacheKey[] key)
        {
            throw new NotImplementedException();
        }

        public T GetOrCreate<T>(CacheKey key, Func<CacheKey, T> cacheValueCreator)
        {
            T value;
            if (this.Get(key, out value))
            {
                return value;
            }
            value = cacheValueCreator(key);
            this.Put(key, value);
            return value;
        }

        public void Remove(CacheKey key)
        {
            this._db.KeyDelete(key);
        }

        public void RemoveRegion(KeyPrefix prefix)
        {
            IEnumerable<RedisKey> keys = this._server.Keys(this._databaseNumber, prefix.SearchPattern);
            this._db.KeyDelete(keys.ToArray());
        }

        public ILock GetLocker()
        {
            return new RedisLocker(this._db);
        }

        public void PutRegion<T>(KeyValuePair<CacheKey, T>[] values)
        {
            KeyValuePair<RedisKey, RedisValue>[] toinsert = values
                 .Select(i => new KeyValuePair<RedisKey, RedisValue>(i.Key, i.Value.ToRedisValue(this._serializer)))
                 .ToArray();
            this._db.StringSet(toinsert);

            // todo : check if this fast ? (latency in each loop)
            foreach (KeyValuePair<CacheKey, T> kv in values.Where(i => i.Key.ExpirationMode != ExpirationMode.None))
            {
                switch (kv.Key.ExpirationMode)
                {
                    case ExpirationMode.Sliding:
                        if (kv.Key.ExpirationTimeSpan.HasValue)
                            this._db.KeyExpire(kv.Key, kv.Key.ExpirationTimeSpan.Value);
                        break;
                    case ExpirationMode.AbsoluteUtc:
                        if (kv.Key.ExpirationDateTime.HasValue)
                            this._db.KeyExpire(kv.Key, kv.Key.ExpirationDateTime.Value);
                        break;
                }
            }
        }

        public void Notify(string channel, Notification notification)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (notification == null) throw new ArgumentNullException(nameof(notification));
            this._subscriber.Publish(channel, notification.ToRedisValue(this._serializer));
        }

        public void Subscribe(string channel, Action<string, Notification> onNotification)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (onNotification == null) throw new ArgumentNullException(nameof(onNotification));
            this._subscriber.Subscribe(channel, (chan, val) => { onNotification(chan, val.FromRedisValue<Notification>(this._serializer)); });
        }
    }
}