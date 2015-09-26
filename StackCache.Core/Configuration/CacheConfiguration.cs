namespace StackCache.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Distributed;
    using Local.Dictionary;
    using Messaging;
    using Serializers;

    public class CacheConfiguration
    {
        public CacheConfiguration()
        {
            this._firstLevelCacheType = FirstLevelCacheType.ConcurrentDictionary;
            this._secondLevelCacheType = SecondLevelCacheType.None;
            this._serializerType = SerializerType.ProtoBufNet;
        }

        private FirstLevelCacheType _firstLevelCacheType;
        private SecondLevelCacheType _secondLevelCacheType;
        private readonly SerializerType _serializerType;
        private int _redisDatabase;
        private List<RedisServer> _redisInstances;

        public Cache CreateCache()
        {
            ISerializer serializer = null;
            switch (this._serializerType)
            {
                case SerializerType.ProtoBufNet:
                    serializer = new ProtoBufSerializer();
                    break;
                case SerializerType.Json:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ICacheAdapter firstLevelCache = null;

            switch (this._firstLevelCacheType)
            {
                case FirstLevelCacheType.None:
                    break;
                case FirstLevelCacheType.ConcurrentDictionary:
                    firstLevelCache = new DictionaryCacheAdapter();
                    break;
                 
                default:
                    throw new ArgumentOutOfRangeException();
            }


            ICacheAdapter secondLevelCache = null;
            IMessenger messenger = null;
            switch (this._secondLevelCacheType)
            {
                case SecondLevelCacheType.None:
                    break;
                case SecondLevelCacheType.Redis:
                    if (this._redisInstances == null || !this._redisInstances.Any())
                        throw new ArgumentNullException("Missing redis instances configurations");
                    var redis = new RedisCacheAdapter(this._redisInstances, this._redisDatabase, serializer);
                    secondLevelCache = redis;
                    messenger = redis;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new Cache(firstLevelCache, secondLevelCache, messenger);
        }

        public CacheConfiguration WithFirstLevel(FirstLevelCacheType type)
        {
            this._firstLevelCacheType = type;
            return this;
        }

        public CacheConfiguration WithSecondLevel(int databaseId, params RedisServer[] servers)
        {
            if (servers == null) throw new ArgumentNullException(nameof(servers));
            this._secondLevelCacheType = SecondLevelCacheType.Redis;
            this._redisDatabase = databaseId;
            this._redisInstances = servers.ToList();
            return this;
        }
    }
}