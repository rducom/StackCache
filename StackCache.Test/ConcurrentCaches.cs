namespace StackCache.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.CacheKeys;
    using Core.Configuration;
    using Xunit;

    public class ConcurrentCaches
    {
        private const string _keySerialized = "42Concurent";
        private const string _keyString = "646846516584";
        private const string _dataString = "skhdksdhiuhxciuhdsiu";
        private static readonly Serialized _dataSerialized = new Serialized { Property = "forty two concurrent property" };

        /// <summary>
        /// Simulate network latency
        /// </summary>
        public static TimeSpan ConcurrentDelay = TimeSpan.FromMilliseconds(1);

        private readonly ICache _cache1;
        private readonly ICache _cache2;

        public ConcurrentCaches()
        {
            CacheConfiguration config = new CacheConfiguration();
            config.WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);
            config.WithSecondLevel(0, new RedisServer { Hostname = "127.0.0.1", Port = 6379 });

            this._cache1 = config.CreateCache();
            this._cache2 = config.CreateCache();
        }

        [Fact]
        public void GetPut()
        {
            this._cache1.Put(_keySerialized, _dataSerialized);
            var found = this._cache2.Get<Serialized>(_keySerialized);
            Assert.Equal(found.Property, _dataSerialized.Property);
        }

        [Fact]
        public async Task GetPutUpdate()
        {
            string key = _keySerialized + "UP";
            this._cache1.Put(key, _dataSerialized);

            await Task.Delay(ConcurrentDelay);

            var found = this._cache2.Get<Serialized>(key);
            Assert.Equal(found.Property, _dataSerialized.Property);

            var initial = this._cache1.Get<Serialized>(key);
            initial.Property = "666";
            this._cache1.Put(key, initial);

            await Task.Delay(ConcurrentDelay);

            var updated = this._cache2.Get<Serialized>(key);
            Assert.Equal(updated.Property, "666");
        }

        [Fact]
        public async Task PutGetStringRegionKeyValueAsync()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);
            Dictionary<CacheKey, string> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()), i => _dataString + i.ToString());
            this._cache1.PutRegion(dictionary.ToArray());

            var fetched = await this._cache2.GetRegionKeyValuesAsync<string>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key], kv.Value);
            }
        }

        [Fact]
        public async Task PutGetStringRegionAsync()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);

            Dictionary<CacheKey, Serialized> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()),
                    i => new Serialized { Id = i, Property = _dataString + i.ToString() });

            this._cache1.PutRegion(dictionary.ToArray());
            Func<Serialized, CacheKey> funk = s => new CacheKey(prefix, s.Id.ToString());
            IEnumerable<Serialized> result = await this._cache2.GetRegionAsync(prefix, funk);
            var fetched = result.ToDictionary(funk);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key].Property, kv.Value.Property);
            }
        }


        [Fact]
        public async Task PutGetSerializedRegionAsync()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);

            Dictionary<CacheKey, Serialized> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()),
                    i => new Serialized { Property = _dataString + i.ToString() });

            this._cache1.PutRegion(dictionary.ToArray());
            var fetched = await this._cache2.GetRegionKeyValuesAsync<Serialized>(prefix);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                if (kv.Key.Suffix == "57")
                {
                    ;
                }
                Assert.Equal(dictionary[kv.Key].Property, kv.Value.Property);
            }
        }

        [Fact]
        public void PutGetSerializedRegion()
        {
            KeyPrefix prefix = new KeyPrefix(Key.Null, _keyString);

            Dictionary<CacheKey, Serialized> dictionary = Enumerable.Range(0, 100)
                .ToDictionary(i => new CacheKey(prefix, i.ToString()),
                    i => new Serialized { Id = i, Property = _dataString + i.ToString() });

            this._cache1.PutRegion(dictionary.ToArray());
            Func<Serialized, CacheKey> funk = s => new CacheKey(prefix, s.Id.ToString());
            IEnumerable<Serialized> result = this._cache2.GetRegion(prefix, funk);
            var fetched = result.ToDictionary(funk);
            Assert.Equal(dictionary.Count(), fetched.Count());
            foreach (var kv in fetched)
            {
                Assert.Equal(dictionary[kv.Key].Property, kv.Value.Property);
            }
        }
    }
}