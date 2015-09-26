namespace StackCache.Test
{
    using System;
    using System.Threading.Tasks;
    using Core;
    using Core.Configuration;
    using Xunit;

    public class ConcurrentCaches
    {
        private const string _keySerialized = "42Concurent";
        private static readonly Serialized _dataSerialized = new Serialized() { Property = "forty two concurrent property" };

        public ConcurrentCaches()
        {
            CacheConfiguration config = new CacheConfiguration();
            config.WithFirstLevel(FirstLevelCacheType.ConcurrentDictionary);
            config.WithSecondLevel(0, new RedisServer() { Hostname = "127.0.0.1", Port = 6379 });

            this._cache1 = config.CreateCache();
            this._cache2 = config.CreateCache();
        }

        private ICache _cache1;
        private ICache _cache2;

        /// <summary>
        /// Simulate network latency
        /// </summary>
        public static TimeSpan ConcurrentDelay = TimeSpan.FromMilliseconds(1);

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
    }
}